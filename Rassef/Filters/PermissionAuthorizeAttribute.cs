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
        public PermissionAuthorizeAttribute(string? controller = "", string? action = "")
            : base(typeof(PermissionAuthorizeFilter))
        {
            Arguments = new object[] { controller ?? string.Empty, action ?? string.Empty };
        }
    }

    public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly string? _requiredController;
        private readonly string? _requiredAction;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Controllers that are exclusively part of the Admin Dashboard.
        /// Regular authenticated users must NOT access these.
        /// </summary>
        private static readonly HashSet<string> AdminOnlyControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Administration",
            "Group",
            "Shift",
            "QueueSettings"
        };

        public PermissionAuthorizeFilter(
            string? requiredController,
            string? requiredAction,
            ApplicationDbContext dbContext)
        {
            _requiredController = requiredController;
            _requiredAction = requiredAction;
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 1. Skip if [AllowAnonymous] is present
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
                return;

            // 2. Require authentication
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

            // 3. Resolve controller and action being accessed
            string currentController = !string.IsNullOrWhiteSpace(_requiredController)
                ? _requiredController
                : (context.RouteData.Values["controller"]?.ToString() ?? "");

            string currentAction = !string.IsNullOrWhiteSpace(_requiredAction)
                ? _requiredAction
                : (context.RouteData.Values["action"]?.ToString() ?? "");

            // 4. Parse user ID
            var userIdStr = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? userPrincipal.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdStr, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Authentication", null);
                return;
            }

            // 5. Load user with group and position (AsNoTracking to avoid tracking conflict in controllers)
            var user = await _dbContext.Users
                .AsNoTracking()
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

            // 6. Determine if user is Admin
            //    Admin = group named "Admin" OR position contains "Admin" OR username is "admin"
            bool isAdmin =
                (user.Group != null && user.Group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.Position != null && user.Position.PositionName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                || (user.UserName != null && user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase));

            // 7. Admins have unrestricted access to ALL controllers
            if (isAdmin)
                return;

            // 8. Non-admin: block access to admin-only dashboard controllers
            if (AdminOnlyControllers.Contains(currentController))
            {
                DenyAccess(context, currentController, currentAction);
                return;
            }

            // 9. Non-admin: allow access to all other controllers freely
            //    (group-based granular permissions are optional; if no specific permission is
            //     required the user passes through; if a specific permission IS required,
            //     verify it via the group's GroupPermissions)
            //
            //    If the attribute was applied WITHOUT specifying a controller/action (i.e. bare
            //    [PermissionAuthorize]), we simply allow any authenticated non-admin user through.
            if (string.IsNullOrWhiteSpace(_requiredController) && string.IsNullOrWhiteSpace(_requiredAction))
            {
                // No specific permission required — authenticated user is allowed
                return;
            }

            // 10. A specific permission was requested — check group permissions
            if (user.Group == null)
            {
                // No group assigned — deny the specific-permission endpoint
                DenyAccess(context, currentController, currentAction);
                return;
            }

            bool hasPermission = user.Group.GroupPermissions.Any(gp =>
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

        private static bool IsAjaxRequest(HttpRequest request) =>
            request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || request.Headers["Accept"].ToString().Contains("application/json")
            || request.ContentType?.Contains("application/json") == true;
    }
}
