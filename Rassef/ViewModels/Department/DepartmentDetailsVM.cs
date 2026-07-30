namespace Rassef.ViewModels.Department
{
    public class DepartmentDetailsVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
