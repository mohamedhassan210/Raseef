namespace Rassef.ViewModels.Warehouse
{
    public class WarehouseDetailsVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;

        public List<string> DepartmentNames { get; set; } = new List<string>();
        public List<string> DockNames { get; set; } = new List<string>();
    }
}