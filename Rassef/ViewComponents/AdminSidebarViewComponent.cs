using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rassef.Data;
using Rassef.Services;
using System.Security.Claims;

namespace Rassef.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        private readonly IUserPermissionService _permissionService;
        private readonly ApplicationDbContext _dbContext;

        public AdminSidebarViewComponent(IUserPermissionService permissionService, ApplicationDbContext dbContext)
        {
            _permissionService = permissionService;
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string active)
        {
            var model = new AdminSidebarVM { Active = active ?? string.Empty };

            model.CanViewEmployees = await _permissionService.HasAsync("Administration", "Index");
            model.CanViewTrucks = await _permissionService.HasAsync("Administration", "TrucksIndex");
            model.CanViewDrivers = await _permissionService.HasAsync("Administration", "DriversIndex");
            model.CanViewSuppliers = await _permissionService.HasAsync("Administration", "Suppliers");
            model.CanViewSupplierRequests = await _permissionService.HasAsync("Administration", "SupplierRequests");
            model.CanViewTransferRequests = await _permissionService.HasAsync("Administration", "TransferRequests");
            model.CanViewGroups = await _permissionService.HasAsync("Group", "GroupManagment");
            model.CanViewRoles = await _permissionService.HasAsync("Group", "MangeRolesIndex");
            model.CanViewOptions = await _permissionService.HasAsync("Group", "MangeTypesIndex");
            model.CanViewShifts = await _permissionService.HasAsync("Shift", "Index");
            model.CanViewPositions = await _permissionService.HasAsync("Position", "Index");
            model.CanViewWarehouses = await _permissionService.HasAsync("Warehouses", "Index");
            model.CanViewDepartments = await _permissionService.HasAsync("Department", "Index");
            model.CanViewDocks = await _permissionService.HasAsync("Dock", "Index");

            // Warehouse dropdown — the view posts the chosen id to
            // AuthenticationController.SelectWarehouse, which persists it to the
            // SelectedWarehouseId cookie plus User.LastPickedWarehouseId.
            var userIdStr = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? HttpContext.User.FindFirst("sub")?.Value;
            int.TryParse(userIdStr, out int userId);

            // Only warehouses this user is actually assigned to (UserWarehouse rows) —
            // NO admin bypass. This deliberately matches the rule
            // AuthenticationController.SelectWarehouse already enforces on POST: it
            // rejects any warehouse the user isn't assigned to, admin or not. Listing
            // every warehouse here (the previous behaviour) meant an admin could pick
            // an option that the server would then refuse to switch to.
            model.AvailableWarehouses = await _dbContext.Set<Rassef.Models.Identity.UserWarehouse>()
                .Where(uw => uw.UserId == userId && !uw.IsDeleted && !uw.Warehouse.IsDeleted)
                .OrderBy(uw => uw.Warehouse.Name)
                .Select(uw => new WarehouseOptionVM { Id = uw.WarehouseId, Name = uw.Warehouse.Name })
                .ToListAsync();

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            model.UserName = user?.Name ?? "مستخدم";

            // Which option should show as chosen. Same precedence the SupOrTra page
            // uses: the cookie wins if it still points at an assigned warehouse,
            // otherwise fall back to the user's last saved pick.
            var accessibleIds = model.AvailableWarehouses.Select(w => w.Id).ToHashSet();

            if (HttpContext.Request.Cookies.TryGetValue("SelectedWarehouseId", out var cookieValue)
                && int.TryParse(cookieValue, out var cookieWarehouseId)
                && accessibleIds.Contains(cookieWarehouseId))
            {
                model.SelectedWarehouseId = cookieWarehouseId;
            }
            else if (user?.LastPickedWarehouseId != null
                && accessibleIds.Contains(user.LastPickedWarehouseId.Value))
            {
                model.SelectedWarehouseId = user.LastPickedWarehouseId;
            }

            return View(model);
        }
    }

    public class AdminSidebarVM
    {
        public string Active { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<WarehouseOptionVM> AvailableWarehouses { get; set; } = new();
        public int? SelectedWarehouseId { get; set; }

        public bool CanViewEmployees { get; set; }
        public bool CanViewTrucks { get; set; }
        public bool CanViewDrivers { get; set; }
        public bool CanViewSuppliers { get; set; }
        public bool CanViewSupplierRequests { get; set; }
        public bool CanViewTransferRequests { get; set; }
        public bool CanViewGroups { get; set; }
        public bool CanViewRoles { get; set; }
        public bool CanViewOptions { get; set; }
        public bool CanViewShifts { get; set; }
        public bool CanViewPositions { get; set; }
        public bool CanViewWarehouses { get; set; }
        public bool CanViewDepartments { get; set; }
        public bool CanViewDocks { get; set; }
    }

    public class WarehouseOptionVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}