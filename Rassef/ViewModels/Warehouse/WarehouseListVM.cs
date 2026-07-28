namespace Rassef.ViewModels.Warehouse
{
    public class WarehouseListVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public int DocksCount { get; set; }
        public int DepartmentsCount { get; set; }
    }
}