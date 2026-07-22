

namespace Rassef.Models.Entities

{
    public class Dock : BaseEntity
    {
        public int DepartmentId { get; set; }
        public string DockName { get; set; }
        public int WarehouseId { get; set; }
        public int DockStatusId { get; set; }
        public User CreatedBy { get; set; }
        public Department Department { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<DockAssignment> DockAssignments { get; set; }
        public Dock_statuses DockStatus { get; set; }


    }
}
