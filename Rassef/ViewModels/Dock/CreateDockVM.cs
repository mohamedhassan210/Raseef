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

        // Feature — dock capacity + maintenance status.
        [Display(Name = "الحد الأقصى لعدد الشاحنات")]
        [Range(1, int.MaxValue, ErrorMessage = "الحد الأقصى لعدد الشاحنات يجب أن يكون 1 أو أكثر.")]
        public int MaxTruckCount { get; set; }

        [Display(Name = "في صيانة")]
        public bool IsUnderMaintenance { get; set; }

        // Feature — department (Id) -> warehouse (Id) lookup, used client-side to
        // filter the department dropdown down to the selected warehouse.
        public Dictionary<int, int>? DepartmentWarehouseMap { get; set; }

        // NEW — lets this page be opened as the "+ add dock" launcher from a
        // department card (Warehouse Details/Edit or Department Details/Update):
        // preselects the department/warehouse and, on success, sends the user
        // back to that card view instead of Dock/Index.
        public string? ReturnUrl { get; set; }
    }
}