using Rassef.Common.Interfaces;
using Rassef.Filters;
using Rassef.ViewModels.Authentication.Identity;
using Rassef.ViewModels.Group;
using System.Linq.Expressions;
// REMOVED: using Rassef.Common.Interfaces.Services.AuthenticationServices;
// Confirmed unused by the team — no type from this namespace is referenced anywhere in this file.
//
// CORRECTION from the previous pass: "using Rassef.ViewModels.Authentication.Identity;" was
// removed in the first refactor on the assumption it was dead. The ManagePermissions view
// (`@model Rassef.ViewModels.Authentication.Identity.GroupPermissionsViewModel`) proves
// GroupPermissionsViewModel actually lives in that namespace, not Rassef.ViewModels.Group — so
// removing it would have broken the build. Restored here. Flagging my own mistake rather than
// quietly fixing it, per the "don't silently patch, explain the change" rule.

namespace Rassef.Controllers
{
    [PermissionAuthorize]
    public class GroupController : Controller
    {
        private readonly IRepository<UserGroup> _groupRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IGroupPermissionRepository _groupPermissionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRepository<DriverTypes> _driverTypesRepo;
        private readonly IRepository<Position> _positionRepo;
        private readonly IRepository<TruckTypes> _truckTypesRepo;
        private readonly IRepository<CommodityTypes> _commodityTypesRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Dock> _dockRepo;
        private readonly IRepository<Warehouse> _warehouseRepo;
        private readonly IRepository<Shift> _shiftRepo;
        private readonly IRepository<PermitTypes> _permitTypesRepo;
        private readonly IRepository<DepartmentTypes> _departmentTypesRepo;

        public GroupController(
            IRepository<UserGroup> groupRepo,
            IRepository<Permission> permissionRepo,
            IGroupPermissionRepository groupPermissionRepo,
            IUserRepository userRepo,
            IRepository<DriverTypes> driverTypesRepo,
            IRepository<Position> positionRepo,
            IRepository<TruckTypes> truckTypesRepo,
            IRepository<CommodityTypes> commodityTypesRepo,
            IRepository<Department> departmentRepo,
            IRepository<Dock> dockRepo,
            IRepository<Warehouse> warehouseRepo,
            IRepository<Shift> shiftRepo,
            IRepository<PermitTypes> permitTypesRepo,
            IRepository<DepartmentTypes> departmentTypesRepo)
        {
            _groupRepo = groupRepo ?? throw new ArgumentNullException(nameof(groupRepo));
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _groupPermissionRepo = groupPermissionRepo ?? throw new ArgumentNullException(nameof(groupPermissionRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _driverTypesRepo = driverTypesRepo ?? throw new ArgumentNullException(nameof(driverTypesRepo));
            _positionRepo = positionRepo ?? throw new ArgumentNullException(nameof(positionRepo));
            _truckTypesRepo = truckTypesRepo ?? throw new ArgumentNullException(nameof(truckTypesRepo));
            _commodityTypesRepo = commodityTypesRepo ?? throw new ArgumentNullException(nameof(commodityTypesRepo));
            _departmentRepo = departmentRepo ?? throw new ArgumentNullException(nameof(departmentRepo));
            _dockRepo = dockRepo ?? throw new ArgumentNullException(nameof(dockRepo));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
            _shiftRepo = shiftRepo ?? throw new ArgumentNullException(nameof(shiftRepo));
            _permitTypesRepo = permitTypesRepo ?? throw new ArgumentNullException(nameof(permitTypesRepo));
            _departmentTypesRepo = departmentTypesRepo ?? throw new ArgumentNullException(nameof(departmentTypesRepo));
        }

        // ==============================================================
        // 1. شاشة إدارة المجموعات (Image 1 - Manage Groups)
        // ==============================================================
        /// <summary>
        /// صفحة إدارة المجموعات واستعراض بطاقات المجموعات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GroupManagment()
        {
            var groups = await _groupRepo.GetAllAsync(q => q.Include(g => g.Users));
            var users = await _userRepo.GetAllAsync();

            ViewBag.AllUsers = users.ToList();
            ViewBag.AllGroups = groups.ToList();

            var model = MapToGroupCards(groups);

            return View(model);
        }

        // ==============================================================
        // 2. شاشة إضافة مجموعة (Image 2 - Add Group)
        // ==============================================================
        /// <summary>
        /// صفحة إضافة مجموعة جديدة (GET)
        /// </summary>
        [HttpGet]
        public IActionResult AddGroup()
        {
            return View();
        }

        /// <summary>
        /// حفظ إضافة مجموعة جديدة (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroup(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "يرجى إدخال اسم المجموعة.";
                return View();
            }

            name = name.Trim();

            // CHANGED (behavior fix - Data integrity): UserGroup.Name is confirmed unique at the
            // DB level. Added the missing pre-check so a duplicate name returns a clean Arabic
            // message instead of surfacing as an unhandled 500 from the DB constraint.
            if (await _groupRepo.ExistsAsync(g => !g.IsDeleted && g.Name == name))
            {
                TempData["ErrorMessage"] = "توجد مجموعة بنفس الاسم بالفعل.";
                return View();
            }

            var group = new UserGroup { Name = name };

            await _groupRepo.AddAsync(group);
            await _groupRepo.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة المجموعة بنجاح!";
            return RedirectToAction(nameof(GroupManagment));
        }

        /// <summary>
        /// حذف / تعطيل مجموعة (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            // Reviewed: no admin-lockout guard added here — confirmed as a deliberate product
            // decision (admins are trusted not to disable the group holding admin permissions),
            // not an oversight.
            var group = await _groupRepo.GetByIdAsync(id);
            if (group != null)
            {
                group.IsDeleted = !group.IsDeleted;
                await _groupRepo.SaveChangesAsync();
                TempData["SuccessMessage"] = group.IsDeleted ? "تم تعطيل المجموعة بنجاح!" : "تم تفعيل المجموعة بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "المجموعة غير موجودة.";
            }

            return RedirectToAction(nameof(GroupManagment));
        }

        /// <summary>
        /// نقل موظف إلى مجموعة معينة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferUser(int userId, int targetGroupId, int? returnGroupId = null)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "الموظف غير موجود.";
            }
            else if (targetGroupId > 0)
            {
                // CHANGED (behavior fix - Data integrity): verify the target group actually exists
                // before assigning it as the user's GroupId FK. Previously an invalid id passed
                // straight to SaveChangesAsync and threw an unhandled FK-violation 500.
                var targetGroup = await _groupRepo.GetByIdAsync(targetGroupId);
                if (targetGroup == null)
                {
                    TempData["ErrorMessage"] = "المجموعة المحددة غير موجودة.";
                }
                else
                {
                    user.GroupId = targetGroupId;
                    await _userRepo.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم نقل الموظف إلى المجموعة بنجاح!";
                }
            }
            else
            {
                user.GroupId = null;
                await _userRepo.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم نقل الموظف إلى المجموعة بنجاح!";
            }

            if (returnGroupId.HasValue && returnGroupId.Value > 0)
            {
                return RedirectToAction(nameof(GroupDetails), new { id = returnGroupId.Value });
            }

            return RedirectToAction(nameof(GroupManagment));
        }

        /// <summary>
        /// صفحة تفاصيل المجموعة واستعراض الموظفين المنتمين إليها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GroupDetails(int id)
        {
            var group = await _groupRepo.GetByIdAsync(id);
            if (group == null)
            {
                TempData["ErrorMessage"] = "هذه المجموعة غير موجودة.";
                return RedirectToAction(nameof(GroupManagment));
            }

            var groupUsers = await _userRepo.GetAllAsync(q => q.Where(u => u.GroupId == id));
            var allUsers = await _userRepo.GetAllAsync();

            ViewBag.AllUsers = allUsers.ToList();
            await PopulateAllGroupsViewBagAsync();

            var model = new GroupDetailsVM
            {
                Id = group.Id,
                Name = group.Name,
                // CHANGED: confirmed against the UserGroup entity source that it has no
                // Description field at all — the previous hardcoded Arabic string ("IT
                // department") was fabricated and shown identically for every group regardless
                // of which one was being viewed. Left blank rather than inventing a replacement;
                // needs a real column added to UserGroup if this should show real content.
                Description = string.Empty,
                CreatedAt = ResolveCreatedAt(group.CreatedAT),
                IsActive = !group.IsDeleted,
                Employees = groupUsers.Select(u => new GroupEmployeeVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Code = !string.IsNullOrWhiteSpace(u.UserCode) ? u.UserCode : (!string.IsNullOrWhiteSpace(u.NationalId) ? u.NationalId : $"{u.Id:D6}"),
                    // CHANGED (behavior fix - Data integrity): a user with no email on file was
                    // previously shown a fabricated "@microsoft.com" address built from their
                    // name — wrong contact info, not a harmless placeholder. Replaced with a
                    // neutral label.
                    Email = u.Email?.Value ?? (u.UserName != null && u.UserName.Contains("@") ? u.UserName : "غير متوفر"),
                    // CHANGED (behavior fix - Data integrity): same issue for the hardcoded fake
                    // phone number fallback.
                    Phone = !string.IsNullOrWhiteSpace(u.Phone) ? u.Phone : "غير متوفر",
                    IsActive = !u.IsDeleted
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// تعديل اسم المجموعة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "تعذر تعديل المجموعة.";
                return RedirectToAction(nameof(GroupDetails), new { id });
            }

            var group = await _groupRepo.GetByIdAsync(id);
            if (group == null)
            {
                TempData["ErrorMessage"] = "تعذر تعديل المجموعة.";
                return RedirectToAction(nameof(GroupDetails), new { id });
            }

            name = name.Trim();

            // CHANGED (behavior fix - Data integrity): same uniqueness check as AddGroup, excluding
            // the current row so renaming a group to its own existing name still works.
            if (await _groupRepo.ExistsAsync(g => g.Id != id && !g.IsDeleted && g.Name == name))
            {
                TempData["ErrorMessage"] = "توجد مجموعة بنفس الاسم بالفعل.";
                return RedirectToAction(nameof(GroupDetails), new { id });
            }

            group.Name = name;
            await _groupRepo.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تعديل اسم المجموعة بنجاح!";
            return RedirectToAction(nameof(GroupDetails), new { id });
        }

        // ==============================================================
        // 3. شاشة إدارة الأدوار (Image 3 - Manage Roles & Permissions Overview)
        // ==============================================================
        /// <summary>
        /// صفحة إدارة الأدوار واستعراض مجموعات الصلاحيات وجدول كل الصلاحيات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MangeRolesIndex()
        {
            // Sync all controllers and actions from Reflection into Permission database table
            await SyncPermissionsFromReflectionAsync();

            var groups = await _groupRepo.GetAllAsync(q => q.Include(g => g.Users));
            var permissions = await _permissionRepo.GetAllAsync();

            var model = new RolesManagementVM
            {
                Groups = MapToGroupCards(groups),

                Permissions = permissions.Select(p => new PermissionTableItemVM
                {
                    Id = p.Id,
                    Controller = p.ControllerName,
                    Action = p.ActionName,
                    Description = !string.IsNullOrWhiteSpace(p.Description) ? p.Description : $"{p.ControllerName}{p.ActionName}"
                }).ToList()
            };

            return View(model);
        }

        private async Task SyncPermissionsFromReflectionAsync()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var controllerTypes = assembly.GetTypes()
                    .Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
                    .ToList();

                var existingPermissions = await _permissionRepo.GetAllAsync();
                // Build a lookup: key="{controller}_{action}" -> Permission entity
                var existingLookup = existingPermissions
                    .ToDictionary(
                        p => $"{p.ControllerName}_{p.ActionName}".ToLowerInvariant(),
                        p => p);

                var newPermissions = new List<Permission>();
                bool anyUpdated = false;

                foreach (var controllerType in controllerTypes)
                {
                    // Strip "Controller" suffix — matches what PermissionAuthorize reads from RouteData
                    string controllerName = controllerType.Name;
                    if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                        controllerName = controllerName[..^"Controller".Length];

                    var actionMethods = controllerType
                        .GetMethods(System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName &&
                                    !m.GetCustomAttributes(typeof(NonActionAttribute), true).Any() &&
                                    m.DeclaringType != typeof(Controller) &&
                                    m.DeclaringType != typeof(ControllerBase) &&
                                    m.DeclaringType != typeof(object) &&
                                    (typeof(IActionResult).IsAssignableFrom(m.ReturnType) ||
                                     typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType) ||
                                     typeof(ActionResult).IsAssignableFrom(m.ReturnType) ||
                                     typeof(Task<ActionResult>).IsAssignableFrom(m.ReturnType)))
                        .GroupBy(m => {
                            var actAttr = m.GetCustomAttributes(typeof(ActionNameAttribute), true).FirstOrDefault() as ActionNameAttribute;
                            return actAttr?.Name ?? m.Name;
                        })
                        .Select(g => g.First());

                    foreach (var method in actionMethods)
                    {
                        var actAttr = method.GetCustomAttributes(typeof(ActionNameAttribute), true).FirstOrDefault() as ActionNameAttribute;
                        string actionName = actAttr?.Name ?? method.Name;
                        string key = $"{controllerName}_{actionName}".ToLowerInvariant();

                        var descAttr = method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true).FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                        var dispAttr = method.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;

                        string description = !string.IsNullOrWhiteSpace(descAttr?.Description)
                            ? descAttr.Description
                            : (!string.IsNullOrWhiteSpace(dispAttr?.DisplayName)
                                ? dispAttr.DisplayName
                                : BuildDescription(controllerName, actionName));

                        if (!existingLookup.TryGetValue(key, out var existing))
                        {
                            // New permission — add it
                            newPermissions.Add(new Permission
                            {
                                ControllerName = controllerName,
                                ActionName = actionName,
                                Description = description
                            });
                        }
                        else if (string.IsNullOrWhiteSpace(existing.Description) ||
                                 existing.Description == $"{existing.ControllerName}{existing.ActionName}")
                        {
                            // Old auto-generated description — refresh it
                            existing.Description = description;
                            _permissionRepo.Update(existing);
                            anyUpdated = true;
                        }
                    }
                }

                if (newPermissions.Any())
                {
                    foreach (var perm in newPermissions)
                        await _permissionRepo.AddAsync(perm);
                    await _permissionRepo.SaveChangesAsync();
                }
                else if (anyUpdated)
                {
                    await _permissionRepo.SaveChangesAsync();
                }
            }
            catch
            {
                // TODO(flag - Consistency/style): broad empty catch still swallows every
                // exception here. Not narrowed — left as a follow-up, not part of this pass.
            }
        }

        /// <summary>
        /// "CreateShift" in controller "Shift" → "Shift - Create Shift"
        /// </summary>
        private static string BuildDescription(string controllerName, string actionName)
        {
            var words = System.Text.RegularExpressions.Regex.Replace(actionName, "([A-Z])", " $1").Trim();
            return $"{controllerName} - {words}";
        }

        // ==============================================================
        // 4. شاشة إدارة صلاحيات المجموعة (Image 4 - Manage Group Permissions)
        // ==============================================================
        /// <summary>
        /// صفحة تخصيص الصلاحيات لمجموعة معينة (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(int groupId)
        {
            var group = await _groupRepo.GetByIdAsync(groupId);
            if (group == null)
            {
                TempData["ErrorMessage"] = "هذه المجموعة غير موجودة";
                return RedirectToAction(nameof(MangeRolesIndex));
            }

            // Sync all controllers and actions from Reflection into Permission database table
            await SyncPermissionsFromReflectionAsync();

            await PopulateAllGroupsViewBagAsync();

            var allPermissions = await _permissionRepo.GetAllAsync();
            var allGroupPermissions = await _groupPermissionRepo.GetByGroupIdAsync(groupId);

            var currentGroupPermissions = allGroupPermissions
                .Select(x => x.PermissionId)
                .ToHashSet();

            var model = new GroupPermissionsViewModel
            {
                GroupId = groupId,
                GroupName = group.Name,
                Controllers = allPermissions
                    .GroupBy(p => p.ControllerName)
                    .Select(g => new ControllerPermissionsViewModel
                    {
                        ControllerName = g.Key,
                        Actions = g.Select(p => new PermissionCheckBoxViewModel
                        {
                            PermissionId = p.Id,
                            ActionName = p.ActionName,
                            Description = !string.IsNullOrWhiteSpace(p.Description) ? p.Description : p.ActionName,
                            IsSelected = currentGroupPermissions.Contains(p.Id)
                        }).ToList()
                    }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// حفظ تعديلات صلاحيات المجموعة (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(GroupPermissionsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // CHANGED (behavior fix - Blocking): repopulate ViewBag.AllGroups here too, since
                // the view's groups nav pill list depends on it and the GET action sets it.
                //
                // Note: checked the view — Controllers[i].Actions[j].ActionName/Description are
                // posted back as hidden fields, so the round-trip on validation failure is fine;
                // no further fix needed there.
                await PopulateAllGroupsViewBagAsync();
                return View(model);
            }

            var selectedPermissionIds = model.Controllers
                .SelectMany(c => c.Actions)
                .Where(a => a.IsSelected)
                .Select(a => a.PermissionId)
                .Distinct()
                .ToList();

            await _groupPermissionRepo.UpdatePermissionsForGroupAsync(model.GroupId, selectedPermissionIds);

            TempData["SuccessMessage"] = "تم تحديث الصلاحيات بنجاح!";
            return RedirectToAction(nameof(MangeRolesIndex));
        }

        // ==============================================================
        // 5. شاشة إدارة الإختيارات (Image 5 - Manage Types / Lookups)
        // ==============================================================
        /// <summary>
        /// صفحة استعراض كل أنواع الإختيارات والقوائم المرجعية
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MangeTypesIndex()
        {
            // CHANGED (Consistency, follows soft-delete adoption below): counts now exclude
            // soft-deleted rows so a "deleted" lookup item doesn't keep inflating the card counts.
            var driverTypesCount = (await _driverTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var positionsCount = (await _positionRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var truckTypesCount = (await _truckTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var commodityTypesCount = (await _commodityTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var departmentsCount = (await _departmentRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var docksCount = (await _dockRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var warehousesCount = (await _warehouseRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var shiftsCount = (await _shiftRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var permitTypesCount = (await _permitTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();

            var model = new TypesManagementVM
            {
                TypeCategories = new List<TypeCardVM>
                {
                    new TypeCardVM { Key = "driver-type", Title = "نوع السائق", Count = driverTypesCount },
                    new TypeCardVM { Key = "position", Title = "دور الموظف", Count = positionsCount },
                    new TypeCardVM { Key = "truck-type", Title = "نوع الشاحنة", Count = truckTypesCount },
                    new TypeCardVM { Key = "commodity-type", Title = "نوع السلعة", Count = commodityTypesCount },
                    new TypeCardVM { Key = "department", Title = "الأقسام", Count = departmentsCount },
                    new TypeCardVM { Key = "dock", Title = "الأرصفة", Count = docksCount },
                    new TypeCardVM { Key = "warehouse", Title = "المستودعات", Count = warehousesCount },
                    new TypeCardVM { Key = "shift", Title = "الورديات", Count = shiftsCount },
                    new TypeCardVM { Key = "permit-type", Title = "أنواع التصاريح", Count = permitTypesCount }
                }
            };

            return View(model);
        }

        // ==============================================================
        // 6. شاشة تفاصيل وإدارة عناصر الإختيار (Image 2 - Lookup Type Details)
        // ==============================================================

        private static readonly HashSet<string> KnownTypeKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "driver-type", "position", "truck-type", "commodity-type",
            "department", "dock", "warehouse", "shift", "permit-type"
        };

        /// <summary>
        ///     صفحة استعراض وتعديل وحذف عناصر نوع معين من الاختيارات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MangeTypeDetails(string typeKey)
        {
            var key = string.IsNullOrWhiteSpace(typeKey) || !KnownTypeKeys.Contains(typeKey)
                ? "driver-type"
                : typeKey.ToLower();

            var model = new TypeDetailsVM { TypeKey = key };

            // CHANGED (Consistency, follows soft-delete adoption below): every branch now filters
            // out soft-deleted rows so a deleted item stops appearing in the list.
            switch (key)
            {
                case "driver-type":
                    model.Title = "نوع السائق";
                    model.Items = MapToTypeItems(await _driverTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "position":
                    model.Title = "دور الموظف";
                    model.Items = MapToTypeItems(await _positionRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.PositionName);
                    break;

                case "truck-type":
                    model.Title = "نوع الشاحنة";
                    model.Items = MapToTypeItems(await _truckTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "commodity-type":
                    model.Title = "نوع السلعة";
                    model.Items = MapToTypeItems(await _commodityTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "department":
                    model.Title = "الأقسام";
                    model.Items = MapToTypeItems(await _departmentRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "dock":
                    model.Title = "الأرصفة";
                    model.Items = MapToTypeItems(await _dockRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.DockName);
                    break;

                case "warehouse":
                    model.Title = "المستودعات";
                    model.Items = MapToTypeItems(await _warehouseRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "shift":
                    model.Title = "الورديات";
                    model.Items = MapToTypeItems(await _shiftRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "permit-type":
                    model.Title = "أنواع التصاريح";
                    model.Items = MapToTypeItems(await _permitTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;
            }

            return View(model);
        }

        /// <summary>
        /// إضافة عنصر جديد إلى القائمة المرجعية المحددة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTypeItem(string typeKey, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "يرجى إدخال اسم الاختيار.";
                return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
            }

            name = name.Trim();
            bool handled = true;

            // CHANGED (behavior fix - Data integrity): every case now checks for a duplicate
            // (non-deleted) name before inserting, confirmed as a real DB unique constraint on all
            // of these Name/DockName columns. Uses the shared DuplicateGuardAsync helper so a
            // duplicate returns the same clean Arabic message pattern already used by Position.
            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    if (await DuplicateGuardAsync(_driverTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع سائق", typeKey) is IActionResult r1) return r1;
                    await AddEntityAsync(_driverTypesRepo, new DriverTypes { Name = name });
                    break;

                case "position":
                    if (await DuplicateGuardAsync(_positionRepo, x => !x.IsDeleted && x.PositionName == name, "دور وظيفي", typeKey) is IActionResult r2) return r2;

                    var allPositions = await _positionRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted));
                    int nextPositionCode = allPositions.Any() ? allPositions.Max(p => p.PositionCode) + 1 : 1;
                    // TODO(flag - Data integrity): PositionCode is computed as max+1 with no
                    // transaction/locking — two concurrent creates could compute the same code.
                    // Not changed here; needs a decision on whether it must be a DB sequence.

                    await AddEntityAsync(_positionRepo, new Position { PositionName = name, PositionCode = nextPositionCode });
                    break;

                case "truck-type":
                    if (await DuplicateGuardAsync(_truckTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع شاحنة", typeKey) is IActionResult r3) return r3;
                    await AddEntityAsync(_truckTypesRepo, new TruckTypes { Name = name });
                    break;

                case "commodity-type":
                    if (await DuplicateGuardAsync(_commodityTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع سلعة", typeKey) is IActionResult r4) return r4;
                    await AddEntityAsync(_commodityTypesRepo, new CommodityTypes { Name = name });
                    break;

                case "department":
                    if (await DuplicateGuardAsync(_departmentRepo, x => !x.IsDeleted && x.Name == name, "قسم", typeKey) is IActionResult r5) return r5;

                    var departmentTypes = await _departmentTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted));
                    var warehouses = await _warehouseRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted));

                    if (!departmentTypes.Any() || !warehouses.Any())
                    {
                        TempData["ErrorMessage"] = "لا يمكن إضافة قسم قبل إضافة نوع قسم ومستودع واحد على الأقل.";
                        return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
                    }

                    // TODO(flag - Blocking/Data integrity, STILL OPEN — required FK hardcoded):
                    // confirmed this should not be hardcoded at all, but fixing it properly means
                    // adding a warehouse/department-type selector to the create form, which is new
                    // UI outside the scope of a controller-only refactor. WarehouseId and
                    // DepartmentTypeId currently still default to `.First()`, and Prefix is still
                    // hardcoded to "A". I need either: (a) the current CreateTypeItem view/modal for
                    // typeKey=department, so I can add the corresponding form fields and wire real
                    // parameters through, or (b) explicit sign-off to change this action's signature
                    // to accept warehouseId/departmentTypeId/prefix and update the call site(s).
                    await AddEntityAsync(_departmentRepo, new Department
                    {
                        Name = name,
                        WarehouseId = warehouses.First().Id,
                        Prefix = "A",
                        DepartmentTypeId = departmentTypes.First().Id
                    });
                    break;

                case "dock":
                    if (await DuplicateGuardAsync(_dockRepo, x => !x.IsDeleted && x.DockName == name, "رصيف", typeKey) is IActionResult r6) return r6;

                    // TODO(flag - Blocking/Data integrity, STILL OPEN — required FK hardcoded):
                    // same as department above — DepartmentId, WarehouseId, and DockStatusId are
                    // still hardcoded to 1. Needs the create-form/view for typeKey=dock (or sign-off
                    // to change the action signature) before I can wire in real selections.
                    await AddEntityAsync(_dockRepo, new Dock { DockName = name, DepartmentId = 1, WarehouseId = 1, DockStatusId = 1 });
                    break;

                case "warehouse":
                    if (await DuplicateGuardAsync(_warehouseRepo, x => !x.IsDeleted && x.Name == name, "مستودع", typeKey) is IActionResult r7) return r7;
                    await AddEntityAsync(_warehouseRepo, new Warehouse { Name = name });
                    break;

                case "shift":
                    if (await DuplicateGuardAsync(_shiftRepo, x => !x.IsDeleted && x.Name == name, "وردية", typeKey) is IActionResult r8) return r8;
                    // TODO(flag - Data integrity): StartTime/Duration still hardcoded (8:00, 8h)
                    // for every new shift — flagged previously, unchanged, needs the create form.
                    await AddEntityAsync(_shiftRepo, new Shift { Name = name, StartTime = TimeSpan.FromHours(8), Duration = TimeSpan.FromHours(8) });
                    break;

                case "permit-type":
                    if (await DuplicateGuardAsync(_permitTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع تصريح", typeKey) is IActionResult r9) return r9;
                    await AddEntityAsync(_permitTypesRepo, new PermitTypes { Name = name });
                    break;

                default:
                    handled = false;
                    break;
            }

            TempData[handled ? "SuccessMessage" : "ErrorMessage"] =
                handled ? "تم إضافة الاختيار بنجاح!" : "نوع الاختيار غير معروف.";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }

        /// <summary>
        /// تعديل عنصر في القائمة المرجعية المحددة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTypeItem(string typeKey, int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "يرجى إدخال اسم الاختيار.";
                return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
            }

            name = name.Trim();
            bool found;

            // CHANGED (behavior fix - Data integrity): duplicate check added to every case
            // (excluding the row being edited), same reasoning as CreateTypeItem above.
            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    if (await DuplicateGuardAsync(_driverTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع سائق", typeKey) is IActionResult e1) return e1;
                    found = await UpdateEntityFieldAsync(_driverTypesRepo, id, e => e.Name = name);
                    break;

                case "position":
                    if (await DuplicateGuardAsync(_positionRepo, x => x.Id != id && !x.IsDeleted && x.PositionName == name, "دور وظيفي", typeKey) is IActionResult e2) return e2;
                    found = await UpdateEntityFieldAsync(_positionRepo, id, e => e.PositionName = name);
                    break;

                case "truck-type":
                    if (await DuplicateGuardAsync(_truckTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع شاحنة", typeKey) is IActionResult e3) return e3;
                    found = await UpdateEntityFieldAsync(_truckTypesRepo, id, e => e.Name = name);
                    break;

                case "commodity-type":
                    if (await DuplicateGuardAsync(_commodityTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع سلعة", typeKey) is IActionResult e4) return e4;
                    found = await UpdateEntityFieldAsync(_commodityTypesRepo, id, e => e.Name = name);
                    break;

                case "department":
                    if (await DuplicateGuardAsync(_departmentRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "قسم", typeKey) is IActionResult e5) return e5;
                    found = await UpdateEntityFieldAsync(_departmentRepo, id, e => e.Name = name);
                    break;

                case "dock":
                    if (await DuplicateGuardAsync(_dockRepo, x => x.Id != id && !x.IsDeleted && x.DockName == name, "رصيف", typeKey) is IActionResult e6) return e6;
                    found = await UpdateEntityFieldAsync(_dockRepo, id, e => e.DockName = name);
                    break;

                case "warehouse":
                    if (await DuplicateGuardAsync(_warehouseRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "مستودع", typeKey) is IActionResult e7) return e7;
                    found = await UpdateEntityFieldAsync(_warehouseRepo, id, e => e.Name = name);
                    break;

                case "shift":
                    if (await DuplicateGuardAsync(_shiftRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "وردية", typeKey) is IActionResult e8) return e8;
                    found = await UpdateEntityFieldAsync(_shiftRepo, id, e => e.Name = name);
                    break;

                case "permit-type":
                    if (await DuplicateGuardAsync(_permitTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع تصريح", typeKey) is IActionResult e9) return e9;
                    found = await UpdateEntityFieldAsync(_permitTypesRepo, id, e => e.Name = name);
                    break;

                default:
                    found = false;
                    break;
            }

            TempData[found ? "SuccessMessage" : "ErrorMessage"] =
                found ? "تم تعديل الاختيار بنجاح!" : "لم يتم العثور على العنصر المطلوب.";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }

        /// <summary>
        /// حذف عنصر من القائمة المرجعية المحددة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTypeItem(string typeKey, int id)
        {
            // CHANGED (behavior fix - Consistency/Data integrity): switched from hard delete
            // (Remove) to soft delete (IsDeleted = true), matching the rest of the codebase and
            // confirmed as the required pattern — "we don't do hard delete at all". This assumes
            // these entity types share the same BaseEntity (with an IsDeleted flag) that UserGroup
            // uses — confirmed for UserGroup's source, but I have not seen the entity files for
            // DriverTypes/Position/TruckTypes/CommodityTypes/Department/Dock/Warehouse/Shift/
            // PermitTypes. If any of them doesn't inherit BaseEntity, this won't compile for that
            // type specifically — let me know which one and I'll adjust just that case.
            //
            // Also: this is a one-way soft delete (no toggle/undelete), since — unlike
            // DeleteGroup — there's no "reactivate" affordance for lookup items today.
            bool found;

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    found = await SoftDeleteEntityAsync(_driverTypesRepo, id);
                    break;
                case "position":
                    found = await SoftDeleteEntityAsync(_positionRepo, id);
                    break;
                case "truck-type":
                    found = await SoftDeleteEntityAsync(_truckTypesRepo, id);
                    break;
                case "commodity-type":
                    found = await SoftDeleteEntityAsync(_commodityTypesRepo, id);
                    break;
                case "department":
                    found = await SoftDeleteEntityAsync(_departmentRepo, id);
                    break;
                case "dock":
                    found = await SoftDeleteEntityAsync(_dockRepo, id);
                    break;
                case "warehouse":
                    found = await SoftDeleteEntityAsync(_warehouseRepo, id);
                    break;
                case "shift":
                    found = await SoftDeleteEntityAsync(_shiftRepo, id);
                    break;
                case "permit-type":
                    found = await SoftDeleteEntityAsync(_permitTypesRepo, id);
                    break;
                default:
                    found = false;
                    break;
            }

            TempData[found ? "SuccessMessage" : "ErrorMessage"] =
                found ? "تم حذف الاختيار بنجاح!" : "لم يتم العثور على العنصر المطلوب.";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }

        // ==============================================================
        // Shared private helpers
        // ==============================================================

        /// <summary>
        /// CreatedAT on UserGroup can apparently be default(DateTimeOffset); preserves the
        /// existing "fall back to DateTime.Now" behavior exactly, just de-duplicated. A default
        /// CreatedAT likely means something upstream isn't setting it on insert — flagged
        /// separately in the follow-up questions, not changed here.
        /// </summary>
        private static DateTime ResolveCreatedAt(DateTimeOffset createdAt) =>
            createdAt != default ? createdAt.DateTime : DateTime.Now;

        private static List<GroupCardVM> MapToGroupCards(IEnumerable<UserGroup> groups) =>
            groups.Select(g => new GroupCardVM
            {
                Id = g.Id,
                Name = g.Name,
                UsersCount = g.Users?.Count ?? 0,
                CreatedAt = ResolveCreatedAt(g.CreatedAT),
                IsActive = !g.IsDeleted
            }).ToList();

        private async Task PopulateAllGroupsViewBagAsync()
        {
            ViewBag.AllGroups = (await _groupRepo.GetAllAsync()).ToList();
        }

        private static List<TypeItemVM> MapToTypeItems<T>(IEnumerable<T> items, Func<T, int> idSelector, Func<T, string> nameSelector) =>
            items.Select(x => new TypeItemVM { Id = idSelector(x), Name = nameSelector(x) }).ToList();

        private static async Task AddEntityAsync<T>(IRepository<T> repo, T entity) where T : class
        {
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
        }

        private static async Task<bool> UpdateEntityFieldAsync<T>(IRepository<T> repo, int id, Action<T> applyChanges) where T : class
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;

            applyChanges(entity);
            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Soft-deletes an entity (IsDeleted = true) instead of removing the row, per the
        /// project's "no hard deletes" rule. Requires T : BaseEntity — see the flag on
        /// DeleteTypeItem above about unverified entity inheritance.
        /// </summary>
        private static async Task<bool> SoftDeleteEntityAsync<T>(IRepository<T> repo, int id) where T : BaseEntity
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks for an existing non-deleted row matching predicate; if found, sets the standard
        /// Arabic duplicate-name error and returns a redirect for the caller to return immediately.
        /// Returns null when there's no duplicate, so the caller proceeds normally.
        /// </summary>
        private async Task<IActionResult?> DuplicateGuardAsync<T>(IRepository<T> repo, Expression<Func<T, bool>> predicate, string itemLabel, string typeKey) where T : class
        {
            if (!await repo.ExistsAsync(predicate))
                return null;

            TempData["ErrorMessage"] = $"يوجد {itemLabel} بنفس الاسم بالفعل.";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }
    }
}