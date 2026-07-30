
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
    }
}