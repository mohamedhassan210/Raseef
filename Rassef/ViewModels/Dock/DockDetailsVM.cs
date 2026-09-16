namespace Rassef.ViewModels.Dock
{
    public class DockDetailsVM
    {
        [Display(Name = "المعرف (ID)")]
        public int Id { get; set; }

        [Display(Name = "اسم الرصيف")]
        public string DockName { get; set; } = string.Empty;
        [Display(Name = "اسم المخزن")]
        public string WarehouseName { get; set; } = string.Empty;
        [Display(Name = "حالة الرصيف")]
        public string DockStatusName { get; set; } = string.Empty;

        [Display(Name = "اسم القسم")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "تم الإنشاء بواسطة")]
        public int CreatedBy { get; set; }

        // Feature — dock capacity + maintenance status.
        [Display(Name = "الحد الأقصى لعدد الشاحنات")]
        public int MaxTruckCount { get; set; }

        [Display(Name = "في صيانة")]
        public bool IsUnderMaintenance { get; set; }

        [Display(Name = "الإشغال الحالي")]
        public int Occupancy { get; set; }

        public bool IsFull => Occupancy >= MaxTruckCount;
    }
}