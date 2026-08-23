using Rassef.ViewModels.Group;

namespace Rassef.Controllers
{
    public class GroupController : Controller
    {
        private readonly IRepository<UserGroup> _groupRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<GroupPermission> _groupPermissionRepo;
        private readonly IRepository<DriverTypes> _driverTypesRepo;
        private readonly IRepository<Position> _positionRepo;
        private readonly IRepository<TruckTypes> _truckTypesRepo;
        private readonly IRepository<CommodityTypes> _commodityTypesRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Dock> _dockRepo;
        private readonly IRepository<Warehouse> _warehouseRepo;
        private readonly IRepository<Shift> _shiftRepo;
        private readonly IRepository<PermitTypes> _permitTypesRepo;

        public GroupController(
            IRepository<UserGroup> groupRepo,
            IRepository<Permission> permissionRepo,
            IRepository<GroupPermission> groupPermissionRepo,
            IRepository<DriverTypes> driverTypesRepo,
            IRepository<Position> positionRepo,
            IRepository<TruckTypes> truckTypesRepo,
            IRepository<CommodityTypes> commodityTypesRepo,
            IRepository<Department> departmentRepo,
            IRepository<Dock> dockRepo,
            IRepository<Warehouse> warehouseRepo,
            IRepository<Shift> shiftRepo,
            IRepository<PermitTypes> permitTypesRepo)
        {
            _groupRepo = groupRepo ?? throw new ArgumentNullException(nameof(groupRepo));
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _groupPermissionRepo = groupPermissionRepo ?? throw new ArgumentNullException(nameof(groupPermissionRepo));
            _driverTypesRepo = driverTypesRepo ?? throw new ArgumentNullException(nameof(driverTypesRepo));
            _positionRepo = positionRepo ?? throw new ArgumentNullException(nameof(positionRepo));
            _truckTypesRepo = truckTypesRepo ?? throw new ArgumentNullException(nameof(truckTypesRepo));
            _commodityTypesRepo = commodityTypesRepo ?? throw new ArgumentNullException(nameof(commodityTypesRepo));
            _departmentRepo = departmentRepo ?? throw new ArgumentNullException(nameof(departmentRepo));
            _dockRepo = dockRepo ?? throw new ArgumentNullException(nameof(dockRepo));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
            _shiftRepo = shiftRepo ?? throw new ArgumentNullException(nameof(shiftRepo));
            _permitTypesRepo = permitTypesRepo ?? throw new ArgumentNullException(nameof(permitTypesRepo));
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

            var model = groups.Select(g => new GroupCardVM
            {
                Id = g.Id,
                Name = g.Name,
                UsersCount = g.Users?.Count ?? 0,
                CreatedAt = g.CreatedAT != default ? g.CreatedAT.DateTime : DateTime.Now,
                IsActive = true
            }).ToList();

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

            var group = new UserGroup
            {
                Name = name.Trim()
            };

            await _groupRepo.AddAsync(group);
            await _groupRepo.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة المجموعة بنجاح!";
            return RedirectToAction(nameof(GroupManagment));
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
            var groups = await _groupRepo.GetAllAsync(q => q.Include(g => g.Users));
            var permissions = await _permissionRepo.GetAllAsync();

            var model = new RolesManagementVM
            {
                Groups = groups.Select(g => new GroupCardVM
                {
                    Id = g.Id,
                    Name = g.Name,
                    UsersCount = g.Users?.Count ?? 0,
                    CreatedAt = g.CreatedAT != default ? g.CreatedAT.DateTime : DateTime.Now,
                    IsActive = true
                }).ToList(),

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

            var allPermissions = await _permissionRepo.GetAllAsync();
            var allGroupPermissions = await _groupPermissionRepo.GetAllAsync();

            var currentGroupPermissions = allGroupPermissions
                .Where(x => x.GroupId == groupId)
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
                return View(model);
            }

            // مسح الصلاحيات القديمة للمجموعة
            var allGroupPermissions = await _groupPermissionRepo.GetAllAsync();
            var existingPermissions = allGroupPermissions.Where(x => x.GroupId == model.GroupId).ToList();

            foreach (var existingPermission in existingPermissions)
            {
                _groupPermissionRepo.Remove(existingPermission);
            }

            // إضافة الصلاحيات المحددة الجديدة
            var selectedPermissions = model.Controllers
                .SelectMany(c => c.Actions)
                .Where(a => a.IsSelected)
                .ToList();

            foreach (var permission in selectedPermissions)
            {
                var newGroupPermission = new GroupPermission
                {
                    GroupId = model.GroupId,
                    PermissionId = permission.PermissionId
                };

                await _groupPermissionRepo.AddAsync(newGroupPermission);
            }

            await _groupPermissionRepo.SaveChangesAsync();

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
            var driverTypesCount = (await _driverTypesRepo.GetAllAsync()).Count();
            var positionsCount = (await _positionRepo.GetAllAsync()).Count();
            var truckTypesCount = (await _truckTypesRepo.GetAllAsync()).Count();
            var commodityTypesCount = (await _commodityTypesRepo.GetAllAsync()).Count();
            var departmentsCount = (await _departmentRepo.GetAllAsync()).Count();
            var docksCount = (await _dockRepo.GetAllAsync()).Count();
            var warehousesCount = (await _warehouseRepo.GetAllAsync()).Count();
            var shiftsCount = (await _shiftRepo.GetAllAsync()).Count();
            var permitTypesCount = (await _permitTypesRepo.GetAllAsync()).Count();

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
        /// <summary>
        /// صفحة استعراض وتعديل وحذف عناصر نوع معين من الاختيارات
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MangeTypeDetails(string typeKey)
        {
            if (string.IsNullOrWhiteSpace(typeKey))
            {
                typeKey = "driver-type";
            }

            var model = new TypeDetailsVM
            {
                TypeKey = typeKey.ToLower()
            };

            switch (typeKey.ToLower())
            {
                case "driver-type":
                    model.Title = "نوع السائق";
                    var dts = await _driverTypesRepo.GetAllAsync();
                    model.Items = dts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "position":
                    model.Title = "دور الموظف";
                    var pos = await _positionRepo.GetAllAsync();
                    model.Items = pos.Select(x => new TypeItemVM { Id = x.Id, Name = x.PositionName }).ToList();
                    break;

                case "truck-type":
                    model.Title = "نوع الشاحنة";
                    var tts = await _truckTypesRepo.GetAllAsync();
                    model.Items = tts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "commodity-type":
                    model.Title = "نوع السلعة";
                    var cts = await _commodityTypesRepo.GetAllAsync();
                    model.Items = cts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "department":
                    model.Title = "الأقسام";
                    var depts = await _departmentRepo.GetAllAsync();
                    model.Items = depts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "dock":
                    model.Title = "الأرصفة";
                    var docks = await _dockRepo.GetAllAsync();
                    model.Items = docks.Select(x => new TypeItemVM { Id = x.Id, Name = x.DockName }).ToList();
                    break;

                case "warehouse":
                    model.Title = "المستودعات";
                    var whs = await _warehouseRepo.GetAllAsync();
                    model.Items = whs.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "shift":
                    model.Title = "الورديات";
                    var shifts = await _shiftRepo.GetAllAsync();
                    model.Items = shifts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                case "permit-type":
                    model.Title = "أنواع التصاريح";
                    var pts = await _permitTypesRepo.GetAllAsync();
                    model.Items = pts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
                    break;

                default:
                    model.Title = "نوع السائق";
                    var defaultDts = await _driverTypesRepo.GetAllAsync();
                    model.Items = defaultDts.Select(x => new TypeItemVM { Id = x.Id, Name = x.Name }).ToList();
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

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    await _driverTypesRepo.AddAsync(new DriverTypes { Name = name });
                    await _driverTypesRepo.SaveChangesAsync();
                    break;

                case "position":
                    await _positionRepo.AddAsync(new Position { PositionName = name });
                    await _positionRepo.SaveChangesAsync();
                    break;

                case "truck-type":
                    await _truckTypesRepo.AddAsync(new TruckTypes { Name = name });
                    await _truckTypesRepo.SaveChangesAsync();
                    break;

                case "commodity-type":
                    await _commodityTypesRepo.AddAsync(new CommodityTypes { Name = name });
                    await _commodityTypesRepo.SaveChangesAsync();
                    break;

                case "department":
                    await _departmentRepo.AddAsync(new Department { Name = name, WarehouseId = 1, Prefix = "A", DepartmentTypeId = 1 });
                    await _departmentRepo.SaveChangesAsync();
                    break;

                case "dock":
                    await _dockRepo.AddAsync(new Dock { DockName = name, DepartmentId = 1, WarehouseId = 1, DockStatusId = 1 });
                    await _dockRepo.SaveChangesAsync();
                    break;

                case "warehouse":
                    await _warehouseRepo.AddAsync(new Warehouse { Name = name });
                    await _warehouseRepo.SaveChangesAsync();
                    break;

                case "shift":
                    await _shiftRepo.AddAsync(new Shift { Name = name, StartTime = TimeSpan.FromHours(8), Duration = TimeSpan.FromHours(8) });
                    await _shiftRepo.SaveChangesAsync();
                    break;

                case "permit-type":
                    await _permitTypesRepo.AddAsync(new PermitTypes { Name = name });
                    await _permitTypesRepo.SaveChangesAsync();
                    break;
            }

            TempData["SuccessMessage"] = "تم إضافة الاختيار بنجاح!";
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

            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    var dt = await _driverTypesRepo.GetByIdAsync(id);
                    if (dt != null) { dt.Name = name; _driverTypesRepo.Update(dt); await _driverTypesRepo.SaveChangesAsync(); }
                    break;

                case "position":
                    var pos = await _positionRepo.GetByIdAsync(id);
                    if (pos != null) { pos.PositionName = name; _positionRepo.Update(pos); await _positionRepo.SaveChangesAsync(); }
                    break;

                case "truck-type":
                    var tt = await _truckTypesRepo.GetByIdAsync(id);
                    if (tt != null) { tt.Name = name; _truckTypesRepo.Update(tt); await _truckTypesRepo.SaveChangesAsync(); }
                    break;

                case "commodity-type":
                    var ct = await _commodityTypesRepo.GetByIdAsync(id);
                    if (ct != null) { ct.Name = name; _commodityTypesRepo.Update(ct); await _commodityTypesRepo.SaveChangesAsync(); }
                    break;

                case "department":
                    var dept = await _departmentRepo.GetByIdAsync(id);
                    if (dept != null) { dept.Name = name; _departmentRepo.Update(dept); await _departmentRepo.SaveChangesAsync(); }
                    break;

                case "dock":
                    var dock = await _dockRepo.GetByIdAsync(id);
                    if (dock != null) { dock.DockName = name; _dockRepo.Update(dock); await _dockRepo.SaveChangesAsync(); }
                    break;

                case "warehouse":
                    var wh = await _warehouseRepo.GetByIdAsync(id);
                    if (wh != null) { wh.Name = name; _warehouseRepo.Update(wh); await _warehouseRepo.SaveChangesAsync(); }
                    break;

                case "shift":
                    var shift = await _shiftRepo.GetByIdAsync(id);
                    if (shift != null) { shift.Name = name; _shiftRepo.Update(shift); await _shiftRepo.SaveChangesAsync(); }
                    break;

                case "permit-type":
                    var pt = await _permitTypesRepo.GetByIdAsync(id);
                    if (pt != null) { pt.Name = name; _permitTypesRepo.Update(pt); await _permitTypesRepo.SaveChangesAsync(); }
                    break;
            }

            TempData["SuccessMessage"] = "تم تعديل الاختيار بنجاح!";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }

        /// <summary>
        /// حذف عنصر من القائمة المرجعية المحددة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTypeItem(string typeKey, int id)
        {
            switch (typeKey?.ToLower())
            {
                case "driver-type":
                    var dt = await _driverTypesRepo.GetByIdAsync(id);
                    if (dt != null) { _driverTypesRepo.Remove(dt); await _driverTypesRepo.SaveChangesAsync(); }
                    break;

                case "position":
                    var pos = await _positionRepo.GetByIdAsync(id);
                    if (pos != null) { _positionRepo.Remove(pos); await _positionRepo.SaveChangesAsync(); }
                    break;

                case "truck-type":
                    var tt = await _truckTypesRepo.GetByIdAsync(id);
                    if (tt != null) { _truckTypesRepo.Remove(tt); await _truckTypesRepo.SaveChangesAsync(); }
                    break;

                case "commodity-type":
                    var ct = await _commodityTypesRepo.GetByIdAsync(id);
                    if (ct != null) { _commodityTypesRepo.Remove(ct); await _commodityTypesRepo.SaveChangesAsync(); }
                    break;

                case "department":
                    var dept = await _departmentRepo.GetByIdAsync(id);
                    if (dept != null) { _departmentRepo.Remove(dept); await _departmentRepo.SaveChangesAsync(); }
                    break;

                case "dock":
                    var dock = await _dockRepo.GetByIdAsync(id);
                    if (dock != null) { _dockRepo.Remove(dock); await _dockRepo.SaveChangesAsync(); }
                    break;

                case "warehouse":
                    var wh = await _warehouseRepo.GetByIdAsync(id);
                    if (wh != null) { _warehouseRepo.Remove(wh); await _warehouseRepo.SaveChangesAsync(); }
                    break;

                case "shift":
                    var shift = await _shiftRepo.GetByIdAsync(id);
                    if (shift != null) { _shiftRepo.Remove(shift); await _shiftRepo.SaveChangesAsync(); }
                    break;

                case "permit-type":
                    var pt = await _permitTypesRepo.GetByIdAsync(id);
                    if (pt != null) { _permitTypesRepo.Remove(pt); await _permitTypesRepo.SaveChangesAsync(); }
                    break;
            }

            TempData["SuccessMessage"] = "تم حذف الاختيار بنجاح!";
            return RedirectToAction(nameof(MangeTypeDetails), new { typeKey });
        }
    }
}
