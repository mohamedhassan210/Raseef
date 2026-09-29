namespace Rassef.ViewModels.Truck
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class AddTraTruckVM
    {
        public string? PlateNumber { get; set; }
        public string? PlateLetter { get; set; }
        public double? StorageCapacity { get; set; }
        public string? TruckType { get; set; }

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public string? NewDriverName { get; set; }
        public string? NewDriverNationalId { get; set; }
        public string? NewDriverPhone { get; set; }

        public int? DepartmentId { get; set; }
        public int? DockId { get; set; }

        // NEW — the department/dock modal now also collects these (previously the
        // controller auto-generated AvizNumber/PermitNumber and picked "any" permit
        // type); TransferRequest already has all three fields, so they're now real
        // user input instead of guessed defaults.
        public string? AvizNumber { get; set; }
        public string? PermitNumber { get; set; }
        public int? PermitTypeId { get; set; }
        public IEnumerable<SelectListItem> PermitTypes { get; set; } = new List<SelectListItem>();

        public IEnumerable<Rassef.ViewModels.Dock.DockOptionVM> AllDocks { get; set; }
            = new List<Rassef.ViewModels.Dock.DockOptionVM>();

        public IEnumerable<SelectListItem> Suppliers { get; set; } = new HashSet<SelectListItem>();
        public IEnumerable<SelectListItem> Drivers { get; set; } = new HashSet<SelectListItem>();
        public int? DriverId { get; set; }
    }
}
