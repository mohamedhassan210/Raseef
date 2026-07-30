namespace Rassef.Controllers
{
    public class GroupController : Controller
    {
        private readonly IRepository<UserGroup> _groupRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<GroupPermission> _groupPermissionRepo;

        public GroupController(
            IRepository<UserGroup> groupRepo,
            IRepository<Permission> permissionRepo,
            IRepository<GroupPermission> groupPermissionRepo)
        {
            _groupRepo = groupRepo ?? throw new ArgumentNullException(nameof(groupRepo));
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _groupPermissionRepo = groupPermissionRepo ?? throw new ArgumentNullException(nameof(groupPermissionRepo));
        }

        // GET: Manage Permissions
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(int groupId)
        {
            var group = await _groupRepo.GetByIdAsync(groupId);
            if (group == null)
            {
                ModelState.AddModelError("", "هذه المجموعة غير موجودة");
                return View(group);
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
                            Description = p.Description,
                            IsSelected = currentGroupPermissions.Contains(p.Id)
                        }).ToList()
                    }).ToList()
            };

            return View(model);
        }

        // POST: Manage Permissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(GroupPermissionsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // مسح الصلاحيات القديمة
            var allGroupPermissions = await _groupPermissionRepo.GetAllAsync();
            var existingPermissions = allGroupPermissions.Where(x => x.GroupId == model.GroupId).ToList();

            foreach (var existingPermission in existingPermissions)
            {
                _groupPermissionRepo.Remove(existingPermission);
            }

            // إضافة الصلاحيات الجديدة المحددة
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

            return RedirectToAction(nameof(Index));
        }
    }
}