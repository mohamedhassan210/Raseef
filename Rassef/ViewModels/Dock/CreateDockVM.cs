
namespace Rassef.ViewModels.Dock
{
    public class CreateDockVM
    {
        [Display(Name = "اسم الرصيف")]
        public string DockName { get; set; } = string.Empty;

        [Display(Name = "القسم")]
        public Guid DepartmentId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Departments { get; set; }

        [Display(Name = "المخزن")]
        public Guid WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

        [Display(Name = "حالة الرصيف")]
        public Guid DockStatusId { get; set; }

        // Status DropDown
        public IEnumerable<SelectListItem>? DockStatus { get; set; }
    }
}