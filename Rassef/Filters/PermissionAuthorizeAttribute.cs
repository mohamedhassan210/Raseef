using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Rassef.Data;

namespace Rassef.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PermissionAuthorizeAttribute : TypeFilterAttribute
    {
        public PermissionAuthorizeAttribute(string? controller = null, string? action = null)
            : base(typeof(PermissionAuthorizeFilter))
        {
            Arguments = new object?[] { controller, action };
        }
    }

    public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly string? _requiredController;
        private readonly string? _requiredAction;
        private readonly ApplicationDbContext _dbContext;

        public PermissionAuthorizeFilter(string? requiredController, string? requiredAction, ApplicationDbContext dbContext)
        {
            _requiredController = requiredController;
            _requiredAction = requiredAction;
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 1. Skip authorization if [AllowAnonymous] is present
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            // 2. Check if user is authenticated
            var userPrincipal = context.HttpContext.User;
            if (userPrincipal?.Identity == null || !userPrincipal.Identity.IsAuthenticated)
            {
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new JsonResult(new { success = false, message = "يرجى تسجيل الدخول أولاً." })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Authentication", null);
                }
                return;
            }

            // 3. Extract Controller and Action names
            string currentController = !string.IsNullOrWhiteSpace(_requiredController)
                ? _requiredController
                : (context.RouteData.Values["controller"]?.ToString() ?? "");

            string currentAction = !string.IsNullOrWhiteSpace(_requiredAction)
                ? _requiredAction
                : (context.RouteData.Values["action"]?.ToString() ?? "");

            // 4. Retrieve User ID from claims
            var userIdStr = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? userPrincipal.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdStr, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Authentication", null);
                return;
            }

            // 5. Query user with their group and assigned permissions
            var user = await _dbContext.Users
                .Include(u => u.Group!)
                    .ThenInclude(g => g.GroupPermissions)
                        .ThenInclude(gp => gp.Permission)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Authentication", null);
                return;
            }

            // 6. Admin Bypass (Full system access)
            bool isAdmin = (user.Group != null && user.Group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.Position != null && user.Position.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.UserName != null && user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                || (user.Name != null && user.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                // Full admin authorization
                return;
            }

            // 7. If user has no group assigned
            if (user.Group == null || user.GroupId == null)
            {
                DenyAccess(context, currentController, currentAction);
                return;
            }

            // 8. Check if group has the required permission in database
            var hasPermission = user.Group.GroupPermissions.Any(gp =>
                gp.Permission != null &&
                gp.Permission.ControllerName.Equals(currentController, StringComparison.OrdinalIgnoreCase) &&
                gp.Permission.ActionName.Equals(currentAction, StringComparison.OrdinalIgnoreCase)
            );

            if (!hasPermission)
            {
                DenyAccess(context, currentController, currentAction);
            }
        }

        private void DenyAccess(AuthorizationFilterContext context, string controller, string action)
        {
            if (IsAjaxRequest(context.HttpContext.Request))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = $"عذراً، ليس لديك صلاحية للوصول إلى ({controller} - {action})."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Authentication", new
                {
                    controllerName = controller,
                    actionName = action
                });
            }
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || request.Headers["Accept"].ToString().Contains("application/json")
                || request.ContentType?.Contains("application/json") == true;
        }
    }
}
