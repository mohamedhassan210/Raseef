namespace Rassef.ViewComponents
{
    /// <summary>
    /// Data the sidebar view needs. Deliberately plain (no [Required] etc.) — this
    /// is a display model, not something bound from a form.
    /// </summary>
    public class UserSidebarViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? CurrentUserName { get; set; }
        public bool IsAdmin { get; set; }
        public List<Warehouse> Warehouses { get; set; } = new();
        public int? SelectedWarehouseId { get; set; }
    }

    /// <summary>
    /// Renders the global "user menu" sidebar: greeting, warehouse picker,
    /// admin-dashboard shortcut (admins only), logout. Meant to be dropped into
    /// any page — including ones spanning several different controllers — via
    /// @await Component.InvokeAsync("UserSidebar"), without that page's own
    /// controller action needing to set anything up for it.
    ///
    /// Deliberately NOT used on Administration/Group/Department/Dock/DriverType/
    /// Position/Shift/TruckTypes/Warehouses pages — those already carry the full
    /// _AdminSidebar partial, which covers the same ground (name/logout) plus the
    /// entire admin nav. Two sidebars on one page would be redundant.
    ///
    /// Read-only by design: unlike AuthenticationController.AddRoleOrView (which can
    /// still re-seed the "SelectedWarehouseId" cookie on its own GET request), this
    /// component may render partway through an already-started response on some
    /// pages, so it never calls Response.Cookies.Append. It only ever *displays*
    /// the effective selection.
    /// </summary>
    public class UserSidebarViewComponent : ViewComponent
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<UserWarehouse> _userWarehouseRepository;

        public UserSidebarViewComponent(
            IUserRepository userRepository,
            IRepository<UserWarehouse> userWarehouseRepository)
        {
            _userRepository = userRepository;
            _userWarehouseRepository = userWarehouseRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new UserSidebarViewModel();

            // NOTE: ViewComponent.User is typed as System.Security.Principal.IPrincipal
            // (unlike Controller.User, which is ClaimsPrincipal), so FindFirstValue isn't
            // available on it directly. HttpContext.User is the properly-typed
            // ClaimsPrincipal — go through that instead.
            var userIdClaim = HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                // Not authenticated (or claim missing) — nothing to show. The view
                // itself checks IsAuthenticated and renders nothing in that case.
                return View(model);
            }

            var allUsers = await _userRepository.GetAllAsync(q => q
                .Include(u => u.Group)
                .Include(u => u.Position));
            var currentUser = allUsers.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return View(model);
            }

            model.IsAuthenticated = true;
            model.CurrentUserName = currentUser.Name;

            // Same admin check as AuthenticationController.IsCurrentUserAdminAsync —
            // kept in sync deliberately since both need to agree on who's an admin.
            model.IsAdmin = (currentUser.Group != null && currentUser.Group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                || (currentUser.Position != null && currentUser.Position.PositionName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(currentUser.UserName) && currentUser.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase));

            var userWarehouseLinks = await _userWarehouseRepository.GetAllAsync(q => q
                .Where(uw => uw.UserId == currentUser.Id && !uw.IsDeleted)
                .Include(uw => uw.Warehouse));

            model.Warehouses = userWarehouseLinks
                .Where(uw => uw.Warehouse != null && !uw.Warehouse.IsDeleted)
                .Select(uw => uw.Warehouse!)
                .ToList();

            int? selectedWarehouseId = null;
            if (HttpContext.Request.Cookies.TryGetValue("SelectedWarehouseId", out var cookieValue)
                && int.TryParse(cookieValue, out var cookieWarehouseId)
                && model.Warehouses.Any(w => w.Id == cookieWarehouseId))
            {
                selectedWarehouseId = cookieWarehouseId;
            }
            else if (currentUser.LastPickedWarehouseId.HasValue
                && model.Warehouses.Any(w => w.Id == currentUser.LastPickedWarehouseId.Value))
            {
                selectedWarehouseId = currentUser.LastPickedWarehouseId.Value;
            }
            model.SelectedWarehouseId = selectedWarehouseId;

            return View(model);
        }
    }
}