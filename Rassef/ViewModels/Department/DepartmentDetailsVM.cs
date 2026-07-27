namespace Rassef.ViewModels.Department
{
    public class DepartmentDetailsVM
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
