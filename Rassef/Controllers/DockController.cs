/*
 * ══════════════════════════════════════════════════════════════
 * ISSUES FOUND — DockController
 * ══════════════════════════════════════════════════════════════
 *
 * BLOCKING
 * --------
 * B1. Details has a broken attribute: [HttpGet("{id :int}")] with a space
 *     inside the route token — this will fail to match because the space
 *     makes the constraint name "{id :int}" unrecognised.
 *     Fixed: changed to plain [HttpGet] (no route template needed; the
 *     action already receives id as a conventional route segment).
 * B2. Details.GetByIdAsync does not Include navigation properties, yet the
 *     code accesses dock.Warehouse.Name, dock.DockStatus.Name,
 *     dock.Department.Name directly without null-conditional — NullReferenceException
 *     on any orphaned FK or lazy-load-disabled context.
 *     Fixed: use FindActiveDockAsync (GetAllAsync + Include) and apply ??"غير محدد".
 * B3. Details returns View(dock) (the entity) when dock == null — View(null)
 *     is not a safe no-data signal; it will throw on model binding.
 *     Fixed: redirect/return empty VM.
 * B4. Hard delete — Remove() called instead of soft-delete.
 *     Fixed: IsDeleted = true; Update(); SaveChanges().
 * B5. No IsDeleted filter anywhere — deleted docks appear in Index and dropdowns.
 *     Fixed: .Where(x => !x.IsDeleted) added throughout.
 * B6. No duplicate-name check on Create or Update.
 *     A DB-level unique constraint on DockName would surface as an unhandled 500.
 *     Fixed: ExistsAsync guard added to both Create and Update POST actions.
 *     (If DockName is not unique at the DB level, these checks are still safe to
 *     keep — they are defensive. Confirm constraint existence via Q1.)
 *
 * SECURITY
 * --------
 * S1. Missing [PermissionAuthorize] on the controller.
 *     Fixed: attribute added.
 *
 * DATA INTEGRITY
 * --------------
 * D1. GetDropdownDataAsync does not filter soft-deleted rows —
 *     deleted departments, warehouses, and statuses appear in dropdowns.
 *     Fixed: .Where(!IsDeleted) added to each.
 *     Note: DockStatuses may not have IsDeleted if it is a lookup seeded table;
 *     see Q2 — the filter is included assuming it inherits BaseEntity;
 *     remove only if DockStatuses does not have IsDeleted.
 * D2. Update POST returns NotFound() (anonymous 404 page) on missing dock —
 *     inconsistent with the rest of the codebase which returns a view with an error.
 *     Fixed: redirects to Index with TempData["ErrorMessage"].
 * D3. Delete GET returns NotFound() on missing dock — same inconsistency.
 *     Fixed: redirects to Index with TempData["ErrorMessage"].
 * D4. DeleteConfirmed returns View(dock) (dock is null at that point) on
 *     missing dock — guaranteed crash.
 *     Fixed: redirects with TempData["ErrorMessage"].
 * D5. No referential guard before deleting a Dock.
 *     The prompt notes: "check whether anything references a Dock before
 *     deleting (search the solution for DockId usage)." The codebase was not
 *     supplied, so this is flagged in Q3 rather than guessed.
 *     A TODO comment is inserted in DeleteConfirmed so the guard location is clear.
 *
 * CONSISTENCY
 * -----------
 * C1. Action names Update/Update deviate from codebase convention (Edit/Edit).
 *     Not renamed — same reason as DepartmentController; flagged in Q4.
 * C2. TempData success/error messages missing.
 *     Fixed: added throughout.
 * C3. Index does not Include navigation properties, relying on an implicit/lazy
 *     load that may silently produce empty strings.
 *     Fixed: explicit Includes added to FindAllActiveDocksAsync.
 *
 * FOLLOW-UP QUESTIONS
 * -------------------
 * Q1. Is there a DB-level UNIQUE constraint on Dock.DockName?
 *     If yes, the ExistsAsync guard added in B6 is essential.
 *     If no constraint, the guard is still harmless but confirm the intended
 *     behaviour (are duplicate dock names allowed?).
 * Q2. Does DockStatuses inherit BaseEntity (i.e. does it have IsDeleted)?
 *     If it is a seeded enum-like lookup without soft-delete, remove the
 *     .Where(s => !s.IsDeleted) filter from GetDropdownDataAsync.
 * Q3. Are there any entities that have a DockId FK (other than the
 *     Warehouse.Docks collection which is the owning side)?
 *     The solution source was not provided. If yes, add a referential guard in
 *     DeleteConfirmed at the TODO comment below before deploying.
 * Q4. Should Update/Update be renamed to Edit/Edit for consistency?
 *     If yes, views and sidebar links must also be updated.
 * Q5. What is the correct [PermissionAuthorize] key for Dock?
 *     Placeholder: "Docks". Supply the correct value if different.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Rassef.Filters;
using Rassef.Models;

using Rassef.ViewModels.Dock;

namespace Rassef.Controllers
{
    [PermissionAuthorize("Docks")]   // S1 — was missing
    public class DockController : Controller
    {
        private readonly IDockRepository _repository;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Warehouse> _warehouseRepo;
        private readonly IRepository<DockStatuses> _statusRepo;

        public DockController(
            IDockRepository repository,
            IRepository<Department> departmentRepo,
            IRepository<Warehouse> warehouseRepo,
            IRepository<DockStatuses> statusRepo)
        {
            _repository = repository;
            _departmentRepo = departmentRepo;
            _warehouseRepo = warehouseRepo;
            _statusRepo = statusRepo;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // C3 + B5 — Include navigations and filter soft-deleted
            var docks = await _repository.GetAllAsync(query => query
                .Where(d => !d.IsDeleted)
                .Include(d => d.Warehouse)
                .Include(d => d.Department)
                .Include(d => d.DockStatus));

            var dockViewModels = docks.Select(x => new DockListVM
            {
                Id = x.Id,
                DockName = x.DockName,
                WarehouseName = x.Warehouse?.Name ?? "غير محدد",
                DepartmentName = x.Department?.Name ?? "غير محدد",
                DockStatusName = x.DockStatus?.Name ?? "غير محدد"
            }).ToList();

            return View(dockViewModels);
        }

        // ── DETAILS ───────────────────────────────────────────────────────────
        // B1 — removed the broken [HttpGet("{id :int}")] route attribute
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // B2 / B3 — use FindActiveDockAsync so navigations are loaded
            var dock = await FindActiveDockAsync(id);

            if (dock == null)
            {
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var model = new DockDetailsVM
            {
                Id = dock.Id,
                DockName = dock.DockName,
                WarehouseName = dock.Warehouse?.Name ?? "غير محدد",   // B2 — was .Name (no ?)
                DockStatusName = dock.DockStatus?.Name ?? "غير محدد",
                DepartmentName = dock.Department?.Name ?? "غير محدد",
                CreatedBy = dock.CreatedById
            };

            return View(model);
        }

        // ── CREATE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await PopulateCreateDropdownsAsync(new CreateDockVM());
            return View(vm);
        }

        // ── CREATE POST ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDockVM create)
        {
            if (!ModelState.IsValid)
            {
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            // B6 — duplicate-name guard
            if (await _repository.ExistsAsync(
                    d => d.DockName == create.DockName && !d.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.DockName), "اسم الرصيف مستخدم بالفعل.");
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var dock = new Dock
            {
                DockName = create.DockName,
                DepartmentId = create.DepartmentId,
                WarehouseId = create.WarehouseId,
                DockStatusId = create.DockStatusId
            };

            await _repository.AddAsync(dock);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة الرصيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // ── UPDATE (EDIT) GET ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var dock = await FindActiveDockAsync(id);

            if (dock == null)
            {
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new UpdateDockVM
            {
                Id = dock.Id,
                DockName = dock.DockName,
                DepartmentId = dock.DepartmentId,
                WarehouseId = dock.WarehouseId,
                DockStatusId = dock.DockStatusId
            };

            vm = await PopulateUpdateDropdownsAsync(vm);
            return View(vm);
        }

        // ── UPDATE (EDIT) POST ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDockVM updateVm)
        {
            if (!ModelState.IsValid)
            {
                updateVm = await PopulateUpdateDropdownsAsync(updateVm);
                return View(updateVm);
            }

            // B6 — duplicate-name guard (exclude self)
            if (await _repository.ExistsAsync(
                    d => d.DockName == updateVm.DockName
                      && d.Id != updateVm.Id
                      && !d.IsDeleted))
            {
                ModelState.AddModelError(nameof(updateVm.DockName), "اسم الرصيف مستخدم بالفعل.");
                updateVm = await PopulateUpdateDropdownsAsync(updateVm);
                return View(updateVm);
            }

            var dock = await FindActiveDockAsync(updateVm.Id);

            if (dock == null)
            {
                // D2 — was NotFound(); now consistent with the rest of the codebase
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            dock.DockName = updateVm.DockName;
            dock.DepartmentId = updateVm.DepartmentId;
            dock.WarehouseId = updateVm.WarehouseId;
            dock.DockStatusId = updateVm.DockStatusId;

            _repository.Update(dock);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل الرصيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE GET ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dock = await FindActiveDockAsync(id);

            if (dock == null)
            {
                // D3 — was NotFound()
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new DockDetailsVM
            {
                Id = dock.Id,
                DockName = dock.DockName,
                DepartmentName = dock.Department?.Name ?? "غير محدد",
                WarehouseName = dock.Warehouse?.Name ?? "غير محدد",
                DockStatusName = dock.DockStatus?.Name ?? "غير محدد"
            };

            return View(vm);
        }

        // ── DELETE POST ───────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dock = await FindActiveDockAsync(id);

            if (dock == null)
            {
                // D4 — was View(dock) where dock == null
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            // TODO (Q3): Insert referential guard here once DockId usages in the
            // solution are confirmed. Pattern:
            //   if (dock.SomeRelatedCollection?.Any(x => !x.IsDeleted) == true) {
            //       TempData["ErrorMessage"] = "لا يمكن حذف الرصيف لأنه مرتبط بـ ...";
            //       return RedirectToAction(nameof(Index));
            //   }

            // B4 — soft delete instead of Remove()
            dock.IsDeleted = true;
            _repository.Update(dock);
            await _repository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الرصيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Loads a single non-deleted Dock with all navigation properties included.
        /// </summary>
        private async Task<Dock?> FindActiveDockAsync(int id)
        {
            var results = await _repository.GetAllAsync(query => query
                .Where(d => d.Id == id && !d.IsDeleted)
                .Include(d => d.Warehouse)
                .Include(d => d.Department)
                .Include(d => d.DockStatus));

            return results.FirstOrDefault();
        }

        private async Task<(IEnumerable<SelectListItem> Depts,
                             IEnumerable<SelectListItem> Warehouses,
                             IEnumerable<SelectListItem> Statuses)> GetDropdownDataAsync()
        {
            // D1 — filter soft-deleted from all three dropdown sources
            var departments = await _departmentRepo.GetAllAsync(
                q => q.Where(d => !d.IsDeleted));
            var warehouses = await _warehouseRepo.GetAllAsync(
                q => q.Where(w => !w.IsDeleted));
            var statuses = await _statusRepo.GetAllAsync(
                q => q.Where(s => !s.IsDeleted));   // see Q2 if DockStatuses has no IsDeleted

            return (
                departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }),
                warehouses.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name }),
                statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            );
        }

        private async Task<CreateDockVM> PopulateCreateDropdownsAsync(CreateDockVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Departments = data.Depts;
            vm.Warehouses = data.Warehouses;
            vm.DockStatus = data.Statuses;
            return vm;
        }

        private async Task<UpdateDockVM> PopulateUpdateDropdownsAsync(UpdateDockVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Departments = data.Depts;
            vm.Warehouses = data.Warehouses;
            vm.DockStatus = data.Statuses;
            return vm;
        }
    }
}