namespace Rassef.Models.Entities
{
    public class Dock : BaseEntity
    {
        public int DepartmentId { get; set; }
        public string DockName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public int DockStatusId { get; set; }
        public User CreatedBy { get; set; } = null!;

    }
}
