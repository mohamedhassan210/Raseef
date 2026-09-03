/*
 * ══════════════════════════════════════════════════════════════
 * ISSUES FOUND — DepartmentController
 * ══════════════════════════════════════════════════════════════
 *
 * BLOCKING
 * --------
 * B1. Routing bug — DeleteConfirmed is [HttpPost] with NO [ActionName("Delete")],
 *     so a form posting to asp-action="Delete" will 404 or hit the GET action.
 *     Fixed: [HttpPost, ActionName("Delete")] added (matches all sibling controllers).
 * B2. Hard delete — Remove() called instead of soft-delete.
 *     Fixed: IsDeleted = true; Update(); SaveChanges().
 * B3. No IsDeleted filter anywhere — deleted departments show in Index and dropdowns,
 *     and their names/prefixes can never be reused.
 *     Fixed: .Where(x => !x.IsDeleted) added to all fetches and ExistsAsync calls.
 * B4. Delete GET renders with no navigation data if the FK is orphaned —
 *     the view only gets a flat VM; no guard for referenced-by check.
 *     Fixed: guard added in DeleteConfirmed (see B5).
 * B5. No referential guard before delete — a department referenced by Docks or
 *     SupplierRequests can be soft-deleted silently.
 *     Fixed: check Department.Docks and Department.SupplierRequests (non-deleted)
 *     before allowing delete; show Arabic error via TempData.
 *     NOTE: this requires IDepartmentRepository.GetByIdAsync (or GetAllAsync)
 *     to include .Docks and .SupplierRequests navigation properties —
 *     see follow-up Q1.
 *
 * SECURITY
 * --------
 * S1. Missing [PermissionAuthorize] on the controller.
 *     Fixed: attribute added.
 * S2. Reset POST action is missing [ValidateAntiForgeryToken].
 *     Fixed: attribute added.
 *
 * DATA INTEGRITY
 * --------------
 * D1. ExistsAsync duplicate-name and duplicate-prefix checks do not exclude
 *     soft-deleted rows.
 *     Fixed: && !x.IsDeleted added to both predicates.
 * D2. GetWarehouseSelectListAsync does not filter soft-deleted warehouses.
 *     Fixed: .Where(w => !w.IsDeleted) added.
 *
 * CONSISTENCY
 * -----------
 * C1. Details and Delete GET use GetAllAsync without filtering soft-deleted rows.
 *     Fixed: !IsDeleted filter applied.
 * C2. Update action name deviates from codebase convention (Edit).
 *     Not changed — the action name is part of the public route; renaming would
 *     break existing links. Flagged in Q2 instead.
 * C3. Reset sets TempData["Success"] but codebase convention is TempData["SuccessMessage"].
 *     Fixed: key renamed to "SuccessMessage".
 * C4. TempData success/error messages missing from Create/Update/Delete.
 *     Fixed: added throughout.
 * C5. DeleteConfirmed (now with [ActionName("Delete")]) still returned View()
 *     on failure instead of redirecting — inconsistent with PositionController.
 *     Fixed: redirects to Index with TempData["ErrorMessage"].
 *
 * FOLLOW-UP QUESTIONS
 * -------------------
 * Q1. Does IDepartmentRepository.GetByIdAsync include .Docks and
 *     .SupplierRequests navigation properties? If not, the referential guard
 *     in DeleteConfirmed will always see empty collections and never block.
 *     Either supply the repository source or confirm these navigations are
 *     loaded — the code below includes them via FindActiveDepartmentAsync which
 *     calls GetAllAsync with explicit Includes; confirm Department has those
 *     navigation properties.
 * Q2. Should "Update" be renamed to "Edit" for consistency with Warehouses/
 *     PositionController? If yes, the sidebar/views also need updating.
 *     Left as-is to avoid breaking existing routes; awaiting confirmation.
 * Q3. What is the correct [PermissionAuthorize] key for Department?
 *     Placeholder: "Departments". Supply the correct value if different.
 * Q4. Department.SupplierRequests navigation — does this exist on the Department
 *     entity? SupplierRequestController references DepartmentId as a scalar FK,
 *     but the inverse navigation may not be declared. Confirm before deploying
 *     the referential check; the guard is wrapped in a null-safe Any() so it
 *     won't crash if the collection is null, but it also won't block if the
 *     navigation isn't loaded.
 * Q5. FIXED (runtime bug) — DepartmentTypeId is a required FK on Department but
 *     CreateDepartmentVM/UpdateDepartmentVM never exposed it and Create/Update
 *     never set it, so it inserted as 0 → FK_Departments_DepartmentTypes_DepartmentTypeId
 *     violation. Added a DepartmentTypeId dropdown to CreateDepartmentVM and Create
 *     (same custom-dropdown pattern as the existing Warehouse field, copied from
 *     Create.cshtml verbatim) so it's a real selection, not a hardcoded default.
 *     NOT added to Update/UpdateDepartmentVM — Update never touched this field
 *     before, so an existing department's type is left untouched on edit; adding
 *     it there is a separate scope decision, flagged below.
 * Q6. FIXED (runtime bug) — CreatedById is also a required FK on Department
 *     (like DepartmentTypeId) but was never set anywhere in this controller either
 *     (untouched by the previous review pass — Warehouse had the same defect,
 *     already fixed there). Resolved the same way: ClaimTypes.NameIdentifier →
 *     IUserRepository.GetByIdAsync → assign to department.CreatedBy, failing
 *     safely (no save, Arabic ModelState error) if the claim/user can't be
 *     resolved, per your instruction on the Warehouse fix.
 * Q7. Should Update/UpdateDepartmentVM also get a DepartmentTypeId dropdown so a
 *     department's type can be changed after creation? Left out of Update for
 *     now since that's a new capability, not a bug in existing behavior — confirm
 *     if you want it added.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using Rassef.Filters;
using Rassef.Models;
using Rassef.Models.Identity;
using Rassef.Models.StatusesAndActions;

using Rassef.ViewModels.Department;

namespace Rassef.Controllers
{
    [PermissionAuthorize("Departments")]   // S1 — was missing
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _repository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<DepartmentTypes> _departmentTypeRepository;   // NEW — Q5
        private readonly IUserRepository _userRepository;                          // NEW — Q6

        public DepartmentController(
            IDepartmentRepository department,
            IRepository<Warehouse> warehouseRepository,
            IRepository<DepartmentTypes> departmentTypeRepository,
            IUserRepository userRepository)
        {
            _repository = department;
            _warehouseRepository = warehouseRepository;
            _departmentTypeRepository = departmentTypeRepository;
            _userRepository = userRepository;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _repository.GetAllAsync(query => query
                .Where(d => !d.IsDeleted)                       // B3
                .Include(d => d.Warehouse));

            var depart = departments.Select(x => new DepartmentListVM
            {
                Id = x.Id,
                Name = x.Name,
                WarehouseName = x.Warehouse?.Name ?? "غير محدد"
            }).ToList();

            return View(depart);
        }

        // ── DETAILS ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var department = await FindActiveDepartmentAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب غير موجود.");
                return View();
            }

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseId = department.WarehouseId,
                WarehouseName = department.Warehouse?.Name ?? "غير محدد"
            };

            return View(model);
        }

        // ── CREATE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateDepartmentVM
            {
                Warehouses = await GetWarehouseSelectListAsync(),
                DepartmentTypes = await GetDepartmentTypeSelectListAsync()   // Q5
            };
            return View(vm);
        }

        // ── CREATE POST ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentVM create)
        {
            if (!ModelState.IsValid)
            {
                create.Warehouses = await GetWarehouseSelectListAsync();
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();   // Q5
                return View(create);
            }

            // D1 — exclude soft-deleted in duplicate checks
            if (await _repository.ExistsAsync(x => x.Name == create.Name && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.Name), "اسم القسم مسجل بالفعل.");
                create.Warehouses = await GetWarehouseSelectListAsync();
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();   // Q5
                return View(create);
            }
            if (await _repository.ExistsAsync(x => x.Prefix == create.Prefix && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.Prefix), "هذا الـ Prefix مستخدم بالفعل.");
                create.Warehouses = await GetWarehouseSelectListAsync();
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();   // Q5
                return View(create);
            }

            // Q6 fix — resolve the current user for CreatedBy, same fail-safe
            // pattern used for Warehouse (no hardcoded fallback id).
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? currentUser = null;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                ModelState.AddModelError("القسم", "تعذر تحديد هوية المستخدم الحالي. يرجى تسجيل الدخول والمحاولة مرة أخرى.");
            }
            else
            {
                currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    ModelState.AddModelError("القسم", "تعذر تحديد هوية المستخدم الحالي. يرجى تسجيل الدخول والمحاولة مرة أخرى.");
                }
            }

            if (!ModelState.IsValid)
            {
                create.Warehouses = await GetWarehouseSelectListAsync();
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();   // Q5
                return View(create);
            }

            var department = new Department
            {
                Name = create.Name,
                Prefix = create.Prefix,
                WarehouseId = create.WarehouseId,
                DepartmentTypeId = create.DepartmentTypeId,   // Q5 — was never set, caused the FK crash
                CreatedBy = currentUser!                       // Q6 — was never set at all
            };

            await _repository.AddAsync(department);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة القسم بنجاح.";  // C4
            return RedirectToAction(nameof(Index));
        }

        // ── UPDATE (EDIT) GET ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var department = await FindActiveDepartmentAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب تعديله غير موجود.");
                return View(new UpdateDepartmentVM
                {
                    Warehouses = await GetWarehouseSelectListAsync()
                });
            }

            var model = new UpdateDepartmentVM
            {
                Id = department.Id,
                Name = department.Name,
                Prefix = department.Prefix,
                WarehouseId = department.WarehouseId,
                Warehouses = await GetWarehouseSelectListAsync()
            };

            return View(model);
        }

        // ── UPDATE (EDIT) POST ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDepartmentVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            var department = await FindActiveDepartmentAsync(model.Id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب تعديله غير موجود.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            // D1 — exclude soft-deleted in duplicate checks
            if (await _repository.ExistsAsync(
                    x => x.Name == model.Name && x.Id != model.Id && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم القسم مسجل بالفعل.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }
            if (await _repository.ExistsAsync(
                    x => x.Prefix == model.Prefix && x.Id != model.Id && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(model.Prefix), "هذا الـ Prefix مستخدم بالفعل.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            department.Name = model.Name;
            department.Prefix = model.Prefix;
            department.WarehouseId = model.WarehouseId;

            _repository.Update(department);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل القسم بنجاح.";  // C4
            return RedirectToAction(nameof(Index));
        }

        // ── RESET ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]   // S2 — was missing
        public async Task<IActionResult> Reset(int id)
        {
            var department = await FindActiveDepartmentAsync(id);

            if (department == null)
            {
                TempData["ErrorMessage"] = "القسم غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            department.LastResetAt = DateTimeOffset.Now;

            _repository.Update(department);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تصفير القسم.";   // C3 — key was "Success"
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await FindActiveDepartmentAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب حذفه غير موجود.");
                return View(new DepartmentDetailsVM());
            }

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseName = department.Warehouse?.Name ?? "غير محدد"
            };

            return View(model);
        }

        // ── DELETE POST ───────────────────────────────────────────────────────
        // B1 — [ActionName("Delete")] was missing; routing would 404 on POST.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await FindActiveDepartmentAsync(id);

            if (department == null)
            {
                TempData["ErrorMessage"] = "القسم المطلوب حذفه غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            // B5 — block delete if department is still referenced by Docks or SupplierRequests
            // NOTE: depends on navigation properties being loaded — see Q1 and Q4 above.
            bool hasDocks = department.Docks?.Any(d => !d.IsDeleted) == true;
            bool hasRequests = department.SupplierRequests?.Any() == true;

            if (hasDocks || hasRequests)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن حذف القسم لأنه مرتبط بأرصفة أو طلبات توريد. " +
                    "يرجى إزالة تلك الارتباطات أولاً.";
                return RedirectToAction(nameof(Index));
            }

            // B2 — soft delete instead of Remove()
            department.IsDeleted = true;
            _repository.Update(department);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف القسم بنجاح.";  // C4 / C5
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Loads a single non-deleted Department with all required navigation properties.
        /// </summary>
        private async Task<Department?> FindActiveDepartmentAsync(int id)
        {
            var results = await _repository.GetAllAsync(query => query
                .Where(d => d.Id == id && !d.IsDeleted)
                .Include(d => d.Warehouse)
                .Include(d => d.Docks)
                .Include(d => d.SupplierRequests));   // for referential guard — see Q1/Q4

            return results.FirstOrDefault();
        }

        private async Task<IEnumerable<SelectListItem>> GetWarehouseSelectListAsync()
        {
            // D2 — exclude soft-deleted warehouses from dropdowns
            var warehouses = await _warehouseRepository.GetAllAsync(
                q => q.Where(w => !w.IsDeleted));

            return warehouses.Select(w =>
                new SelectListItem { Value = w.Id.ToString(), Text = w.Name });
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentTypeSelectListAsync()
        {
            // Q5 — real selection for the required DepartmentTypeId FK
            var departmentTypes = await _departmentTypeRepository.GetAllAsync(
                q => q.Where(t => !t.IsDeleted));

            return departmentTypes.Select(t =>
                new SelectListItem { Value = t.Id.ToString(), Text = t.Name });
        }
    }
}           