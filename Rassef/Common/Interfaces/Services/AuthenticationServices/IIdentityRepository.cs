namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IIdentityRepository : IRepository<UserGroup>
    {
        Task<IEnumerable<GroupPermission>> GetAllGroupPermissionsAsync();

        Task<bool> AddPermissionToGroupAsync(Guid groupId, Guid permissionId);

        Task<bool> RemovePermissionFromGroupAsync(Guid groupId, Guid permissionId);
    }
}
