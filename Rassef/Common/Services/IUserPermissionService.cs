namespace Rassef.Services
{
    public interface IUserPermissionService
    {
        Task<bool> IsAdminAsync();
        Task<HashSet<string>> GetPermissionKeysAsync();
        Task<bool> HasAsync(string controller, string action);
    }
}