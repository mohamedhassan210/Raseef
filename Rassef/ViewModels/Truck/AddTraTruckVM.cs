namespace Rassef.ViewModels.Truck
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

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

        public IEnumerable<SelectListItem> Suppliers { get; set; } = new HashSet<SelectListItem>();
        public IEnumerable<SelectListItem> Drivers { get; set; } = new HashSet<SelectListItem>();
        public int? DriverId { get; set; }
    }
}
