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

            // 8. Decide whether this request needs a real, granular GroupPermission check.
            //
            //    The attribute has two usage styles in this codebase:
            //      a) [PermissionAuthorize("Controller", "Action")] / [PermissionAuthorize("Controller")]
            //         — an explicit permission key was given, so we always check it.
            //      b) bare [PermissionAuthorize] — normally means "any authenticated user is
            //         fine, no specific permission needed" (e.g. the general navigation actions
            //         on AuthenticationController). BUT a handful of controllers
            //         (PermissionPolicy.ExplicitPermissionControllers) are decorated with the bare
            //         form purely for convenience while their actions are still meant to be
            //         individually grantable per group via Group/ManagePermissions — Administration,
            //         Group, Shift, QueueSettings. For those, we still resolve the real
            //         controller/action from the route and check it for real, instead of letting
            //         every authenticated user through and instead of hardcoding a blanket deny.
            bool needsGranularCheck =
                !string.IsNullOrWhiteSpace(_requiredController) ||
                !string.IsNullOrWhiteSpace(_requiredAction) ||
                PermissionPolicy.ExplicitPermissionControllers.Contains(currentController);

            if (!needsGranularCheck)
            {
                // Bare [PermissionAuthorize] on a controller that isn't one of the
                // explicit-permission controllers — authenticated user is allowed.
                return;
            }

            // 9. A specific permission is required — check the user's group permissions.
            //    No controller is ever hardcoded to deny here; access is entirely driven by
            //    whatever GroupPermission rows have actually been granted to the user's group.
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
