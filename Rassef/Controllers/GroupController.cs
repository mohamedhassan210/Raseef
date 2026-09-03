using Rassef.Common.Interfaces;
using Rassef.Filters;
using Rassef.ViewModels.Authentication.Identity;
using Rassef.ViewModels.Group;
using System.Linq.Expressions;
// REMOVED: using Rassef.Common.Interfaces.Services.AuthenticationServices; (confirmed unused)
//
// CHANGED (this pass — scope reduction, not a bug fix): Position, Shift, Warehouse, Department,
// and Dock have all been pulled out of this controller's generic "type management" system.
// Confirmed via the actual PositionController / ShiftController / WarehousesController /
// DepartmentController / DockController that each of these already has its own full CRUD with
// the real dropdowns/uniqueness checks this generic system couldn't provide (it was hardcoding
// required FKs to fill the gap). Keeping both versions alive would mean two code paths writing
// to the same tables with different validation — an actual data-integrity risk on its own — so
// the duplicate cases are removed here rather than left dormant. Same reasoning applies to the
// _positionRepo/_departmentRepo/_dockRepo/_warehouseRepo/_shiftRepo/_departmentTypesRepo
// constructor dependencies below — none of them are used by anything remaining in this file, so
// they're removed too. This is a constructor signature change; since these are populated by the
// DI container (not called directly anywhere), no explicit call site needs updating, but flagging
// it per the "changed signature" rule regardless.

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
        private readonly IRepository<TruckTypes> _truckTypesRepo;
        private readonly IRepository<CommodityTypes> _commodityTypesRepo;
        private readonly IRepository<PermitTypes> _permitTypesRepo;

        public GroupController(
            IRepository<UserGroup> groupRepo,
            IRepository<Permission> permissionRepo,
            IGroupPermissionRepository groupPermissionRepo,
            IUserRepository userRepo,
            IRepository<DriverTypes> driverTypesRepo,
            IRepository<TruckTypes> truckTypesRepo,
            IRepository<CommodityTypes> commodityTypesRepo,
            IRepository<PermitTypes> permitTypesRepo)
        {
            _groupRepo = groupRepo ?? throw new ArgumentNullException(nameof(groupRepo));
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _groupPermissionRepo = groupPermissionRepo ?? throw new ArgumentNullException(nameof(groupPermissionRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _driverTypesRepo = driverTypesRepo ?? throw new ArgumentNullException(nameof(driverTypesRepo));
            _truckTypesRepo = truckTypesRepo ?? throw new ArgumentNullException(nameof(truckTypesRepo));
            _commodityTypesRepo = commodityTypesRepo ?? throw new ArgumentNullException(nameof(commodityTypesRepo));
            _permitTypesRepo = permitTypesRepo ?? throw new ArgumentNullException(nameof(permitTypesRepo));
        }

        // ==============================================================
        // 1. شاشة إدارة المجموعات (Image 1 - Manage Groups)
        // ==============================================================
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
        [HttpGet]
        public IActionResult AddGroup()
        {
            return View();
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
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
                Description = string.Empty,
                CreatedAt = ResolveCreatedAt(group.CreatedAT),
                IsActive = !group.IsDeleted,
                Employees = groupUsers.Select(u => new GroupEmployeeVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Code = !string.IsNullOrWhiteSpace(u.UserCode) ? u.UserCode : (!string.IsNullOrWhiteSpace(u.NationalId) ? u.NationalId : $"{u.Id:D6}"),
                    Email = u.Email?.Value ?? (u.UserName != null && u.UserName.Contains("@") ? u.UserName : "غير متوفر"),
                    Phone = !string.IsNullOrWhiteSpace(u.Phone) ? u.Phone : "غير متوفر",
                    IsActive = !u.IsDeleted
                }).ToList()
            };

            return View(model);
        }

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
        [HttpGet]
        public async Task<IActionResult> MangeRolesIndex()
        {
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
                var existingLookup = existingPermissions
                    .ToDictionary(
                        p => $"{p.ControllerName}_{p.ActionName}".ToLowerInvariant(),
                        p => p);

                var newPermissions = new List<Permission>();
                bool anyUpdated = false;

                foreach (var controllerType in controllerTypes)
                {
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
                // TODO(flag - Consistency/style): broad empty catch, unchanged from earlier passes.
            }
        }

        private static string BuildDescription(string controllerName, string actionName)
        {
            var words = System.Text.RegularExpressions.Regex.Replace(actionName, "([A-Z])", " $1").Trim();
            return $"{controllerName} - {words}";
        }

        // ==============================================================
        // 4. شاشة إدارة صلاحيات المجموعة (Image 4 - Manage Group Permissions)
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(int groupId)
        {
            var group = await _groupRepo.GetByIdAsync(groupId);
            if (group == null)
            {
                TempData["ErrorMessage"] = "هذه المجموعة غير موجودة";
                return RedirectToAction(nameof(MangeRolesIndex));
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(GroupPermissionsViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
        // CHANGED: only name-only lookups remain here now — driver-type, truck-type,
        // commodity-type, permit-type. Position/Shift/Warehouse/Department/Dock moved to their
        // own controllers (see file header comment).
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> MangeTypesIndex()
        {
            var driverTypesCount = (await _driverTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var truckTypesCount = (await _truckTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var commodityTypesCount = (await _commodityTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();
            var permitTypesCount = (await _permitTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted))).Count();

            var model = new TypesManagementVM
            {
                TypeCategories = new List<TypeCardVM>
                {
                    new TypeCardVM { Key = "driver-type", Title = "نوع السائق", Count = driverTypesCount },
                    new TypeCardVM { Key = "truck-type", Title = "نوع الشاحنة", Count = truckTypesCount },
                    new TypeCardVM { Key = "commodity-type", Title = "نوع السلعة", Count = commodityTypesCount },
                    new TypeCardVM { Key = "permit-type", Title = "أنواع التصاريح", Count = permitTypesCount }
                }
            };

            return View(model);
        }

        // ==============================================================
        // 6. شاشة تفاصيل وإدارة عناصر الإختيار
        // ==============================================================

        private static readonly HashSet<string> KnownTypeKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "driver-type", "truck-type", "commodity-type", "permit-type"
        };

        [HttpGet]
        public async Task<IActionResult> MangeTypeDetails(string typeKey)
        {
            var key = string.IsNullOrWhiteSpace(typeKey) || !KnownTypeKeys.Contains(typeKey)
                ? "driver-type"
                : typeKey.ToLower();

            var model = new TypeDetailsVM { TypeKey = key };

            switch (key)
            {
                case "driver-type":
                    model.Title = "نوع السائق";
                    model.Items = MapToTypeItems(await _driverTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "truck-type":
                    model.Title = "نوع الشاحنة";
                    model.Items = MapToTypeItems(await _truckTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "commodity-type":
                    model.Title = "نوع السلعة";
                    model.Items = MapToTypeItems(await _commodityTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;

                case "permit-type":
                    model.Title = "أنواع التصاريح";
                    model.Items = MapToTypeItems(await _permitTypesRepo.GetAllAsync(q => q.Where(x => !x.IsDeleted)), x => x.Id, x => x.Name);
                    break;
            }

            return View(model);
        }

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

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    if (await DuplicateGuardAsync(_driverTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع سائق", typeKey) is IActionResult r1) return r1;
                    await AddEntityAsync(_driverTypesRepo, new DriverTypes { Name = name });
                    break;

                case "truck-type":
                    if (await DuplicateGuardAsync(_truckTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع شاحنة", typeKey) is IActionResult r2) return r2;
                    await AddEntityAsync(_truckTypesRepo, new TruckTypes { Name = name });
                    break;

                case "commodity-type":
                    if (await DuplicateGuardAsync(_commodityTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع سلعة", typeKey) is IActionResult r3) return r3;
                    await AddEntityAsync(_commodityTypesRepo, new CommodityTypes { Name = name });
                    break;

                case "permit-type":
                    if (await DuplicateGuardAsync(_permitTypesRepo, x => !x.IsDeleted && x.Name == name, "نوع تصريح", typeKey) is IActionResult r4) return r4;
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

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    if (await DuplicateGuardAsync(_driverTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع سائق", typeKey) is IActionResult e1) return e1;
                    found = await UpdateEntityFieldAsync(_driverTypesRepo, id, e => e.Name = name);
                    break;

                case "truck-type":
                    if (await DuplicateGuardAsync(_truckTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع شاحنة", typeKey) is IActionResult e2) return e2;
                    found = await UpdateEntityFieldAsync(_truckTypesRepo, id, e => e.Name = name);
                    break;

                case "commodity-type":
                    if (await DuplicateGuardAsync(_commodityTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع سلعة", typeKey) is IActionResult e3) return e3;
                    found = await UpdateEntityFieldAsync(_commodityTypesRepo, id, e => e.Name = name);
                    break;

                case "permit-type":
                    if (await DuplicateGuardAsync(_permitTypesRepo, x => x.Id != id && !x.IsDeleted && x.Name == name, "نوع تصريح", typeKey) is IActionResult e4) return e4;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTypeItem(string typeKey, int id)
        {
            bool found;

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    found = await SoftDeleteEntityAsync(_driverTypesRepo, id);
                    break;
                case "truck-type":
                    found = await SoftDeleteEntityAsync(_truckTypesRepo, id);
                    break;
                case "commodity-type":
                    found = await SoftDeleteEntityAsync(_commodityTypesRepo, id);
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

        private static async Task<bool> SoftDeleteEntityAsync<T>(IRepository<T> repo, int id) where T : BaseEntity
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        private async Task<IActionResult?> DuplicateGuardAsync<T>(IRepository<T> repo, Expression<Func<T, bool>> predicate, string itemLabel, string typeKey) where T : class
        {
            if (!await repo.ExistsAsync(predicate))
                return null;

            TempData["ErrorMessage"] = $"يوجد {itemLabel} بنفس الاسم بالفعل.";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }
    }
}