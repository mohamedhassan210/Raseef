using Microsoft.EntityFrameworkCore;
using Rassef.Common.Interfaces;
using Rassef.Data;
using Rassef.Models.Identity;

namespace Rassef.Common.Repository.EntitiesRepository
{
    public class GroupPermissionRepository : Repository<GroupPermission>, IGroupPermissionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GroupPermissionRepository(ApplicationDbContext db) : base(db)
        {
            _dbContext = db;
        }

        public async Task<IReadOnlyList<GroupPermission>> GetByGroupIdAsync(int groupId)
        {
            return await _dbContext.GroupPermissions
                .Where(gp => gp.GroupId == groupId)
                .ToListAsync();
        }

        public async Task UpdatePermissionsForGroupAsync(int groupId, IEnumerable<int> selectedPermissionIds)
        {
            var selectedIdsSet = selectedPermissionIds.Distinct().ToHashSet();

            var existing = await _dbContext.GroupPermissions
                .Where(gp => gp.GroupId == groupId)
                .ToListAsync();

            var existingIds = existing.Select(gp => gp.PermissionId).ToHashSet();

            // 1. Remove unchecked permissions
            var toRemove = existing
                .Where(gp => !selectedIdsSet.Contains(gp.PermissionId))
                .ToList();

            if (toRemove.Any())
            {
                _dbContext.GroupPermissions.RemoveRange(toRemove);
            }

            // 2. Add newly checked permissions
            var toAdd = selectedIdsSet
                .Where(id => !existingIds.Contains(id))
                .Select(id => new GroupPermission
                {
                    GroupId = groupId,
                    PermissionId = id
                })
                .ToList();

            if (toAdd.Any())
            {
                await _dbContext.GroupPermissions.AddRangeAsync(toAdd);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
