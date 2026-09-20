namespace Rassef.ViewModels.Dock
{
    public class CreateDockVM
    {
        [Display(Name = "اسم الرصيف")]
        public string DockName { get; set; } = string.Empty;

        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Departments { get; set; }

        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

        [Display(Name = "حالة الرصيف")]
        public int DockStatusId { get; set; }

        // Status DropDown
        public IEnumerable<SelectListItem>? DockStatus { get; set; }

        [Display(Name = "أقصى عدد شاحنات")]
        [Range(1, 100, ErrorMessage = "أقصى عدد شاحنات لازم يكون رقم من 1 إلى 100.")]
        public int MaxTruckCount { get; set; } = 1;

        [Display(Name = "تحت الصيانة؟")]
        public bool IsUnderMaintenance { get; set; }

        // Feature — department (Id) -> warehouse (Id) lookup, used client-side to
        // filter the department dropdown down to the selected warehouse.
        public Dictionary<int, int>? DepartmentWarehouseMap { get; set; }
    }
}