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

        // Feature — dock capacity + maintenance. أقصى عدد شاحنات ممكن يشغلوا
        // الرصيف ده في نفس الوقت، وفلاج الصيانة اللي بيمنع اختيار الرصيف
        // خالص (حتى لو فاضي) لحد ما حد يشيله يدوياً.
        public int MaxTruckCount { get; set; } = 1;
        public bool IsUnderMaintenance { get; set; } = false;

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        public ICollection<DockAssignment> DockAssignments { get; set; } = new HashSet<DockAssignment>();
    }
}