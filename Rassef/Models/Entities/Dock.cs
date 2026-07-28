namespace Rassef.Models.Entities
{
    public class Dock : BaseEntity
    {
        public string DockName { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }

        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public Guid DockStatusId { get; set; }
        public DockStatuses DockStatus { get; set; }
        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        public ICollection<DockAssignment> DockAssignments { get; set; } = new HashSet<DockAssignment>();
    }
}