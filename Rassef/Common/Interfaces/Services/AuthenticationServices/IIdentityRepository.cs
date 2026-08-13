namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IIdentityRepository : IRepository<UserGroup>
    {
        Task<IEnumerable<GroupPermission>> GetAllGroupPermissionsAsync();

        Task<bool> AddPermissionToGroupAsync(int groupId, int permissionId);

        Task<bool> RemovePermissionFromGroupAsync(int groupId, int permissionId);
    }
}
