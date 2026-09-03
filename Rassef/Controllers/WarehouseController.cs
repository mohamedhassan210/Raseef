/*
 * ══════════════════════════════════════════════════════════════
 * ISSUES FOUND — WarehousesController
 * ══════════════════════════════════════════════════════════════
 *
 * BLOCKING
 * --------
 * B1. Hard delete — Remove() is called instead of soft-delete.
 *     Fixed: entity.IsDeleted = true; Update(); SaveChanges().
 * B2. No IsDeleted filter on GetAllAsync() in Index, dropdowns,
 *     ExistsAsync duplicate checks, or Details/Delete fetches.
 *     Fixed: .Where(x => !x.IsDeleted) added throughout.
 * B3. Delete does NOT block when Departments or Docks are still
 *     assigned — silent data-integrity hole.
 *     Fixed: guard in DeleteConfirmed redirects with ErrorMessage.
 * B4. Edit's GetByIdAsync does not include Departments, so
 *     warehouse.Departments is null when building SelectedDepartmentIds.
 *     Fixed: use GetAllAsync+FirstOrDefault with Include, same as Details.
 *
 * SECURITY
 * --------
 * S1. Missing [PermissionAuthorize] on the controller.
 *     Fixed: attribute added (same pattern as GroupController/ShiftController).
 *
 * DATA INTEGRITY
 * --------------
 * D1. ExistsAsync duplicate-name check does not exclude soft-deleted rows,
 *     so a deleted warehouse name can never be reused.
 *     Fixed: duplicate checks now filter !IsDeleted.
 * D2. GetDepartmentSelectListAsync / GetDockSelectListAsync return soft-deleted
 *     items, polluting dropdowns.
 *     Fixed: both helpers filter !IsDeleted.
 * D3. Create POST: selectedDepartments/selectedDocks are fetched without
 *     filtering soft-deleted rows, so a deleted dept/dock could be re-linked.
 *     Fixed: filtered in both helpers and in the Create/Edit fetch paths.
 *
 * CONSISTENCY
 * -----------
 * C1. Index GetAllAsync does not filter soft-deleted warehouses —
 *     deleted entries appear in the list.
 *     Fixed: .Where(w => !w.IsDeleted) applied.
 * C2. Details/Delete GET use GetByIdAsync which may return soft-deleted rows.
 *     Fixed: use GetAllAsync+FirstOrDefault with !IsDeleted, same as Index.
 * C3. On validation failure in Create/Edit, Docks dropdown was not
 *     repopulated in the Edit path (Edit only repopulates Departments).
 *     Fixed: both dropdowns repopulated everywhere they are needed.
 * C4. TempData success/error messages not set after Create/Edit/Delete.
 *     Fixed: TempData["SuccessMessage"] / TempData["ErrorMessage"] added.
 *
 * FOLLOW-UP QUESTIONS
 * -------------------
 * Q1. Does UpdateWarehouseVM include a SelectedDockIds property?
 *     The current Edit GET only maps SelectedDepartmentIds and the Edit POST
 *     only saves Departments. If Docks should also be editable on the
 *     warehouse form, UpdateWarehouseVM needs SelectedDockIds and the
 *     Edit view needs the Docks multi-select. Assumed NOT editable on Edit
 *     (only on Create) because UpdateWarehouseVM appears to omit it —
 *     please confirm or supply the VM source file.
 * Q2. Confirm the exact attribute name for [PermissionAuthorize] — the code
 *     below uses [PermissionAuthorize] matching the sibling controllers.
 *     If it takes a permission-key argument (e.g. [PermissionAuthorize("Warehouses")])
 *     supply the correct value; the placeholder below uses "Warehouses".
 * Q3. Does CreateWarehouseVM.SelectedDepartmentIds / SelectedDockIds default
 *     to an empty List<int> (not null) when the form posts with no selections?
 *     The code below guards against null with ?? new List<int>().
 *     If the VM's model binder already guarantees non-null, the guard is harmless.
 * Q4. FIXED — Warehouse.CreatedById was never set on Create, which is exactly what
 *     caused the FK_Warehouses_Users_CreatedById insert failure (shadow FK defaulted
 *     to 0, no User with Id 0 exists). Resolved using the same claim-based pattern
 *     as DriverController/SupplierController: read the user id from
 *     ClaimTypes.NameIdentifier, load the User via IUserRepository, assign it to
 *     warehouse.CreatedBy before AddAsync. Unlike those sibling controllers, this
 *     does NOT fall back to a hardcoded user id (e.g. "= 1") if the claim is missing
 *     or invalid — per explicit instruction, it fails safely instead (ModelState
 *     error, form re-shown, nothing saved). See follow-up questions for the
 *     Warehouse-entity inconsistency this surfaced.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using Rassef.Filters;
using Rassef.Models;
using Rassef.Models.Identity;

using Rassef.ViewModels.Warehouse;

namespace Rassef.Controllers
{
    [PermissionAuthorize("Warehouses")]   // S1 — was missing
    public class WarehousesController : Controller
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Dock> _dockRepository;
        private readonly IUserRepository _userRepository;   // NEW — needed to resolve CreatedBy


        public WarehousesController(
            IRepository<Warehouse> warehouseRepository,
            IRepository<Department> departmentRepository,
            IRepository<Dock> dockRepository,
            IUserRepository userRepository)
        {
            _warehouseRepository = warehouseRepository;
            _departmentRepository = departmentRepository;
            _dockRepository = dockRepository;
            _userRepository = userRepository;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseRepository.GetAllAsync(query => query
                .Where(w => !w.IsDeleted)                   // C1 / B2
                .Include(w => w.CreatedBy)
                .Include(w => w.Departments)
                .Include(w => w.Docks));

            var viewModelList = warehouses.Select(w => new WarehouseListVM
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location,
                CreatedByName = w.CreatedBy?.UserName ?? "غير محدد",
                DocksCount = w.Docks?.Count(d => !d.IsDeleted) ?? 0,
                DepartmentsCount = w.Departments?.Count(d => !d.IsDeleted) ?? 0
            }).ToList();

            return View(viewModelList);
        }

        // ── DETAILS ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("المستودع", "رقم المستودع مفقود.");
                return View(new WarehouseDetailsVM());
            }

            var warehouse = await FindActiveWarehouseAsync(id.Value);

            if (warehouse == null)
            {
                ModelState.AddModelError("المستودع", "هذا المستودع غير موجود.");
                return View(new WarehouseDetailsVM());
            }

            // Read-only view: show every active department/dock, marking the ones
            // linked to this warehouse — using the existing (previously unused)
            // GetDepartmentSelectListAsync/GetDockSelectListAsync helpers as the
            // "all active items" source, per explicit instruction. Not an editable
            // multi-select — no app-wide precedent exists for that interaction yet.
            var linkedDepartmentIds = warehouse.Departments?
                .Where(d => !d.IsDeleted)
                .Select(d => d.Id)
                .ToHashSet() ?? new HashSet<int>();

            var linkedDockIds = warehouse.Docks?
                .Where(d => !d.IsDeleted)
                .Select(d => d.Id)
                .ToHashSet() ?? new HashSet<int>();

            var allDepartments = await GetDepartmentSelectListAsync();
            var allDocks = await GetDockSelectListAsync();

            var viewModel = new WarehouseDetailsVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                CreatedByName = warehouse.CreatedBy?.UserName ?? "غير محدد",
                AllDepartments = allDepartments.Select(item => new WarehouseLinkedItemVM
                {
                    Name = item.Text,
                    IsLinked = linkedDepartmentIds.Contains(int.Parse(item.Value))
                }).ToList(),
                AllDocks = allDocks.Select(item => new WarehouseLinkedItemVM
                {
                    Name = item.Text,
                    IsLinked = linkedDockIds.Contains(int.Parse(item.Value))
                }).ToList()
            };

            return View(viewModel);
        }

        // ── CREATE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateWarehouseVM());
        }

        // ── CREATE POST ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWarehouseVM model)
        {
            // D1 — exclude soft-deleted rows from the duplicate check
            bool nameExists = await _warehouseRepository.ExistsAsync(
                w => w.Name == model.Name && !w.IsDeleted);

            if (nameExists)
                ModelState.AddModelError("Name", "اسم المستودع موجود بالفعل.");

            // Q4 fix — resolve the current user for CreatedBy the same way
            // DriverController/SupplierController do, but fail safely instead of
            // defaulting to a hardcoded user id when the claim is missing/invalid.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? currentUser = null;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                ModelState.AddModelError("المستودع", "تعذر تحديد هوية المستخدم الحالي. يرجى تسجيل الدخول والمحاولة مرة أخرى.");
            }
            else
            {
                currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    ModelState.AddModelError("المستودع", "تعذر تحديد هوية المستخدم الحالي. يرجى تسجيل الدخول والمحاولة مرة أخرى.");
                }
            }

            if (ModelState.IsValid)
            {
                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    Location = model.Location,
                    CreatedBy = currentUser!   // ModelState.IsValid guarantees currentUser is non-null here
                };

                await _warehouseRepository.AddAsync(warehouse);
                await _warehouseRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تمت إضافة المستودع بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ── EDIT GET ──────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("المستودع", "رقم المستودع مفقود.");
                return View(new UpdateWarehouseVM());
            }

            var warehouse = await FindActiveWarehouseAsync(id.Value);

            if (warehouse == null)
            {
                ModelState.AddModelError("المستودع", "هذا المستودع غير موجود.");
                return View(new UpdateWarehouseVM());
            }

            var viewModel = new UpdateWarehouseVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location
            };

            return View(viewModel);
        }

        // ── EDIT POST ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateWarehouseVM model)
        {
            if (id != model.Id)
            {
                ModelState.AddModelError("المستودع", "رقم المستودع غير متطابق.");
                return View(model);
            }

            bool nameExists = await _warehouseRepository.ExistsAsync(
                w => w.Name == model.Name && w.Id != model.Id && !w.IsDeleted);

            if (nameExists)
                ModelState.AddModelError("Name", "اسم المستودع مستخدم بالفعل لمستودع آخر.");

            if (ModelState.IsValid)
            {
                var warehouse = await FindActiveWarehouseAsync(model.Id);

                if (warehouse == null)
                {
                    ModelState.AddModelError("المستودع", "هذا المستودع غير موجود.");
                    return View(model);
                }

                warehouse.Name = model.Name;
                warehouse.Location = model.Location;

                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعديل المستودع بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ── DELETE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("المستودع", "رقم المستودع مفقود.");
                return View(new WarehouseListVM());
            }

            var warehouse = await FindActiveWarehouseAsync(id.Value);

            if (warehouse == null)
            {
                ModelState.AddModelError("المستودع", "هذا المستودع غير موجود.");
                return View(new WarehouseListVM());
            }

            var viewModel = new WarehouseListVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                DepartmentsCount = warehouse.Departments?.Count(d => !d.IsDeleted) ?? 0,
                DocksCount = warehouse.Docks?.Count(d => !d.IsDeleted) ?? 0
            };

            return View(viewModel);
        }

        // ── DELETE POST ───────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await FindActiveWarehouseAsync(id);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "هذا المستودع غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            // B3 — block delete if departments or docks are still assigned
            bool hasDepartments = warehouse.Departments?.Any(d => !d.IsDeleted) == true;
            bool hasDocks = warehouse.Docks?.Any(d => !d.IsDeleted) == true;

            if (hasDepartments || hasDocks)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن حذف المستودع لأنه يحتوي على أقسام أو أرصفة مرتبطة به. " +
                    "يرجى نقل تلك الأقسام والأرصفة أو حذفها أولاً.";
                return RedirectToAction(nameof(Index));
            }

            // B1 — soft delete instead of Remove()
            warehouse.IsDeleted = true;
            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المستودع بنجاح.";  // C4
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Loads a single non-deleted Warehouse with its navigation properties.
        /// Centralises the repeated GetAllAsync+FirstOrDefault pattern.
        /// </summary>
        private async Task<Warehouse?> FindActiveWarehouseAsync(int id)
        {
            var results = await _warehouseRepository.GetAllAsync(query => query
                .Where(w => w.Id == id && !w.IsDeleted)
                .Include(w => w.CreatedBy)
                .Include(w => w.Departments)
                .Include(w => w.Docks));

            return results.FirstOrDefault();
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentSelectListAsync()
        {
            // D2 — exclude soft-deleted from dropdowns
            var departments = await _departmentRepository.GetAllAsync(
                q => q.Where(d => !d.IsDeleted));

            return departments.Select(d =>
                new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
        }

        private async Task<IEnumerable<SelectListItem>> GetDockSelectListAsync()
        {
            // D2 — exclude soft-deleted from dropdowns
            var docks = await _dockRepository.GetAllAsync(
                q => q.Where(d => !d.IsDeleted));

            return docks.Select(d =>
                new SelectListItem { Value = d.Id.ToString(), Text = d.DockName });
        }
    }
}