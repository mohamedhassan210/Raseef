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
using Rassef.Filters;
using Rassef.Models;
using Rassef.Models.Identity;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.Department;
using Rassef.ViewModels.Shared;
using Rassef.ViewModels.Warehouse;
using System.Security.Claims;

namespace Rassef.Controllers
{
    [PermissionAuthorize("Warehouses")]   // S1 — was missing
    public class WarehousesController : Controller
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Dock> _dockRepository;
        private readonly IUserRepository _userRepository;   // NEW — needed to resolve CreatedBy
        private readonly IRepository<DepartmentTypes> _departmentTypeRepository;   // NEW — quick-create department from a warehouse


        public WarehousesController(
            IRepository<Warehouse> warehouseRepository,
            IRepository<Department> departmentRepository,
            IRepository<Dock> dockRepository,
            IUserRepository userRepository,
            IRepository<DepartmentTypes> departmentTypeRepository)
        {
            _warehouseRepository = warehouseRepository;
            _departmentRepository = departmentRepository;
            _dockRepository = dockRepository;
            _userRepository = userRepository;
            _departmentTypeRepository = departmentTypeRepository;
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

            // Read-only view: docks (physical bays) are still shown as a simple
            // linked/not-linked list, same as before. Departments are now shown
            // as cards (each carrying its own Docks as nested mini-cards)
            // instead of the old flat linked-list — per explicit instruction to
            // replace "the current way" for departments. This whole card
            // section is read-only here — add/remove only happens on Edit.
            var linkedDockIds = warehouse.Docks?
                .Where(d => !d.IsDeleted)
                .Select(d => d.Id)
                .ToHashSet() ?? new HashSet<int>();

            var allDocks = await GetDockSelectListAsync();

            var viewModel = new WarehouseDetailsVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                CreatedByName = warehouse.CreatedBy?.UserName ?? "غير محدد",
                DepartmentCards = BuildDepartmentCards(warehouse),
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

        // ── CREATE DEPARTMENT (from inside a warehouse) GET ─────────────────────
        // NEW — quick-create flow, same idea as
        // SupplierRequest/CreateTruckWithDriver?supplierId=...: launched from the
        // parent record's page (here, Warehouses/Details) with the parent id in the
        // route, so the created child is tied to that parent automatically instead
        // of asking the user to pick it again from a big dropdown.
        [HttpGet]
        public async Task<IActionResult> CreateDepartment(int warehouseId)
        {
            var warehouse = await FindActiveWarehouseAsync(warehouseId);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "هذا المستودع غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CreateDepartmentVM
            {
                WarehouseId = warehouse.Id,
                DepartmentTypes = await GetDepartmentTypeSelectListAsync()
            };

            ViewBag.WarehouseName = warehouse.Name;
            return View(vm);
        }

        // ── CREATE DEPARTMENT (from inside a warehouse) POST ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(CreateDepartmentVM create)
        {
            var warehouse = await FindActiveWarehouseAsync(create.WarehouseId);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "هذا المستودع غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();
                ViewBag.WarehouseName = warehouse.Name;
                return View(create);
            }

            // Same duplicate-name/prefix guards as DepartmentController.Create,
            // so a department created from here behaves identically to one
            // created from Department/Create.
            if (await _departmentRepository.ExistsAsync(x => x.Name == create.Name && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.Name), "اسم القسم مسجل بالفعل.");
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();
                ViewBag.WarehouseName = warehouse.Name;
                return View(create);
            }
            if (await _departmentRepository.ExistsAsync(x => x.Prefix == create.Prefix && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.Prefix), "هذا الـ Prefix مستخدم بالفعل.");
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();
                ViewBag.WarehouseName = warehouse.Name;
                return View(create);
            }

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
                create.DepartmentTypes = await GetDepartmentTypeSelectListAsync();
                ViewBag.WarehouseName = warehouse.Name;
                return View(create);
            }

            var department = new Department
            {
                Name = create.Name,
                Prefix = create.Prefix,
                WarehouseId = warehouse.Id,   // always the warehouse this page was opened from — never trusts a posted value
                DepartmentTypeId = create.DepartmentTypeId,
                CreatedBy = currentUser!
            };

            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة القسم إلى المستودع بنجاح.";
            return RedirectToAction(nameof(Details), new { id = warehouse.Id });
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
                Location = warehouse.Location,
                // Same department+doc cards shown on Details — per explicit
                // instruction to show them on Edit too (display-only here;
                // add/remove goes through their own dedicated actions below,
                // not through this form's POST).
                DepartmentCards = BuildDepartmentCards(warehouse)
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
                await RepopulateDepartmentCardsAsync(model);
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
                    await RepopulateDepartmentCardsAsync(model);
                    return View(model);
                }

                warehouse.Name = model.Name;
                warehouse.Location = model.Location;

                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعديل المستودع بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            await RepopulateDepartmentCardsAsync(model);
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

        // ── REMOVE DEPARTMENT CARD (hard delete, "X" button) ─────────────────
        // NEW — per explicit instruction: the department card's "X" button
        // hard-deletes the department (not the app-wide soft delete). Every FK
        // onto Department is DeleteBehavior.Restrict, so this fails loudly with
        // a friendly message instead of a raw SQL error if the department still
        // has docks/docs/tickets/transfer-requests attached.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDepartmentCard(int id, int warehouseId, string? returnAction)
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null || department.WarehouseId != warehouseId)
            {
                TempData["ErrorMessage"] = "هذا القسم غير موجود.";
                return RedirectBackToWarehouse(warehouseId, returnAction);
            }

            try
            {
                _departmentRepository.HardDelete(department);
                await _departmentRepository.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف القسم نهائيًا.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن حذف هذا القسم نهائيًا لوجود بيانات مرتبطة به (أرصفة أو طلبات توريد أو تذاكر). " +
                    "يرجى إزالة تلك الارتباطات أولاً.";
            }

            return RedirectBackToWarehouse(warehouseId, returnAction);
        }

        // ── REMOVE DOCK CARD (hard delete, "X" button) ────────────────────────
        // NEW — same idea as above, but for a Dock (رصيف) nested inside a
        // department card on the Warehouse Details/Edit pages. Only reachable
        // from Edit — Details renders these cards read-only (no X/+ buttons) —
        // but this action stays available regardless of where the form was
        // rendered, the same as RemoveDepartmentCard above.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDockCard(int id, int warehouseId, string? returnAction)
        {
            var dock = await _dockRepository.GetByIdAsync(id);

            if (dock == null || dock.WarehouseId != warehouseId)
            {
                TempData["ErrorMessage"] = "هذا الرصيف غير موجود.";
                return RedirectBackToWarehouse(warehouseId, returnAction);
            }

            try
            {
                _dockRepository.HardDelete(dock);
                await _dockRepository.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الرصيف نهائيًا.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن حذف هذا الرصيف نهائيًا لوجود تعيينات مرتبطة به. يرجى إزالة تلك التعيينات أولاً.";
            }

            return RedirectBackToWarehouse(warehouseId, returnAction);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Sends the user back to whichever page ("Details" or "Edit") the
        /// card's X button was clicked from. Defaults to Details if not given
        /// or not recognised, since that's the safer of the two to land on.
        /// </summary>
        private IActionResult RedirectBackToWarehouse(int warehouseId, string? returnAction)
        {
            var action = returnAction == "Edit" ? "Edit" : "Details";
            return RedirectToAction(action, new { id = warehouseId });
        }

        /// <summary>
        /// Re-fetches and rebuilds DepartmentCards on the Edit VM after a
        /// validation failure, since the cards aren't part of the posted form.
        /// </summary>
        private async Task RepopulateDepartmentCardsAsync(UpdateWarehouseVM model)
        {
            var warehouse = await FindActiveWarehouseAsync(model.Id);
            model.DepartmentCards = warehouse != null ? BuildDepartmentCards(warehouse) : new List<DepartmentCardVM>();
        }

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
                    .ThenInclude(d => d.Docks)
                        .ThenInclude(dk => dk.DockStatus)
                .Include(w => w.Docks));

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Builds the department+dock cards shown on Warehouses/Details and
        /// Warehouses/Edit. Soft-deleted departments/docks are filtered out
        /// here (in-memory, same convention as the rest of this controller)
        /// rather than via a filtered Include.
        /// </summary>
        private static List<DepartmentCardVM> BuildDepartmentCards(Warehouse warehouse)
        {
            return warehouse.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentCardVM
                {
                    Id = d.Id,
                    Name = d.Name,
                    Prefix = d.Prefix,
                    Docks = d.Docks
                        .Where(dk => !dk.IsDeleted)
                        .OrderBy(dk => dk.DockName)
                        .Select(dk => new DockCardVM
                        {
                            Id = dk.Id,
                            DockName = dk.DockName,
                            DockStatusName = dk.DockStatus?.Name ?? "غير محدد"
                        }).ToList()
                }).ToList();
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

        private async Task<IEnumerable<SelectListItem>> GetDepartmentTypeSelectListAsync()
        {
            var departmentTypes = await _departmentTypeRepository.GetAllAsync(
                q => q.Where(t => !t.IsDeleted));

            return departmentTypes.Select(t =>
                new SelectListItem { Value = t.Id.ToString(), Text = t.Name });
        }
    }
}