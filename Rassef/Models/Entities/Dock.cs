namespace Rassef.Models.Entities
{
    public class Dock : BaseEntity
    {
        public string DockName { get; set; } = string.Empty;  

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int DockStatusId { get; set; }
        public DockStatuses DockStatus { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        public ICollection<DockAssignment> DockAssignments { get; set; } = new HashSet<DockAssignment>();
    }
}