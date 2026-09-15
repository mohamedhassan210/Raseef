using Microsoft.EntityFrameworkCore;
using Rassef.Data;
using Rassef.Filters;
using System.Security.Claims;

namespace Rassef.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private bool _resolved;
        private bool _isAdmin;
        private HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);

        public UserPermissionService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsAdminAsync()
        {
            await ResolveAsync();
            return _isAdmin;
        }

        public async Task<HashSet<string>> GetPermissionKeysAsync()
        {
            await ResolveAsync();
            return _keys;
        }

        public async Task<bool> HasAsync(string controller, string action)
        {
            await ResolveAsync();

            if (_isAdmin)
                return true;

            // Same hard denylist PermissionAuthorizeFilter enforces — a granted
            // GroupPermission row can never open these up for a non-admin.
            if (PermissionPolicy.AdminOnlyControllers.Contains(controller))
                return false;

            return _keys.Contains($"{controller}|{action}");
        }

        private async Task ResolveAsync()
        {
            if (_resolved) return;
            _resolved = true;

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var userIdStr = userPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? userPrincipal?.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return;

            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Group!)
                    .ThenInclude(g => g.GroupPermissions)
                        .ThenInclude(gp => gp.Permission)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            // Same admin definition as PermissionAuthorizeFilter — kept in sync manually
            // since it isn't (yet) extracted into PermissionPolicy; flagging that as a
            // follow-up DRY opportunity rather than doing it silently here.
            _isAdmin =
                (user.Group != null && user.Group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.Position != null && user.Position.PositionName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.UserName != null && user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase));

            if (user.Group?.GroupPermissions != null)
            {
                foreach (var gp in user.Group.GroupPermissions)
                {
                    if (gp.Permission != null)
                        _keys.Add($"{gp.Permission.ControllerName}|{gp.Permission.ActionName}");
                }
            }
        }
    }
}