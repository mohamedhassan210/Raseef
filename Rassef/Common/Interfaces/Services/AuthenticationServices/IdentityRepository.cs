

namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public class IdentityRepository : Repository<UserGroup>, IIdentityRepository
    {
        private readonly ApplicationDbContext _context;

        public IdentityRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<bool> AddPermissionToGroupAsync(int groupId, int permissionId)
        {
            var group = await _context.UserGroups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                return false;
            var permission = await _context.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId);
            if (permission == null) return false;

            var exist = await _context.GroupPermissions.AnyAsync(x =>
                     x.GroupId == groupId &&
                     x.PermissionId == permissionId);

            if (exist) return false;
            _context.GroupPermissions.Add(new GroupPermission
            {
                GroupId = groupId,
                PermissionId = permissionId
            });
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<IEnumerable<GroupPermission>> GetAllGroupPermissionsAsync()
        {
            return await _context.GroupPermissions
                      .Include(x => x.Group)
                      .Include(x => x.Permission)
                      .AsNoTracking()
                      .ToListAsync();
        }

        public async Task<bool> RemovePermissionFromGroupAsync(int groupId, int permissionId)
        {
            var groupPermission = await _context.GroupPermissions.FirstOrDefaultAsync(x => x.GroupId == groupId && x.PermissionId == permissionId);

            if (groupPermission == null) return false;

            _context.Remove(groupPermission);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
