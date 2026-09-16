using Rassef.ViewModels.Shared;

namespace Rassef.ViewModels.Department
{
    public class DepartmentDetailsVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int WarehouseId { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        // Docks (ارصفة) that belong to this department, shown as cards on
        // this page — same card format used inside a department card on
        // Warehouses/Details.
        public List<DockCardVM> Docks { get; set; } = new List<DockCardVM>();
    }
}