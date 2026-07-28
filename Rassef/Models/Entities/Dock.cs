namespace Rassef.Models.Entities
{
    public class Dock : BaseEntity
    {
        public Guid DepartmentId { get; set; }
        public string DockName { get; set; } = string.Empty;
        public Department Department { get; set; }
        public Guid DockStatusId { get; set; }
        public User CreatedBy { get; set; } = null!;
        public Warehouse Warehouse { get; set; }
        public DockStatuses DockStatus { get; set; }
        public ICollection<DockAssignment> DockAssignments { get; set; } = new HashSet<DockAssignment>();
    }
}
