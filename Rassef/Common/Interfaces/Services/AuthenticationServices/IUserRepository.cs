namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> AddUserToGroupAsync(User user, Guid groupId);
        Task<bool> RemoveUserFromGroupAsync(User user, Guid groupId);
        Task<bool> ChangeUserGroupAsync(Guid userId, Guid newGroupId);
    }
}
