namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> AddUserToGroupAsync(User user, int groupId);
        Task<bool> RemoveUserFromGroupAsync(User user, int groupId);
        Task<bool> ChangeUserGroupAsync(int userId, int newGroupId);
    }
}
