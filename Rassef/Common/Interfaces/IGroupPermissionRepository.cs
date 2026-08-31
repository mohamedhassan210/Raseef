using Rassef.Common.Interfaces.Services;
using Rassef.Models.Identity;

namespace Rassef.Common.Interfaces
{
    public interface IGroupPermissionRepository : IRepository<GroupPermission>
    {
        Task<IReadOnlyList<GroupPermission>> GetByGroupIdAsync(int groupId);
        Task UpdatePermissionsForGroupAsync(int groupId, IEnumerable<int> selectedPermissionIds);
    }
}
