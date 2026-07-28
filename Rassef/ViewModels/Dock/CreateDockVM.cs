
namespace Rassef.ViewModels.Dock
{
    public class CreateDockVM
    {
        [Required(ErrorMessage = "يرجى إدخال اسم الرصيف.")]
        [StringLength(100, ErrorMessage = "اسم الرصيف يجب ألا يتجاوز 100 حرف.")]
        [Display(Name = "اسم الرصيف")]
        public string DockName { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى اختيار القسم.")]
        [Display(Name = "القسم")]
        public Guid DepartmentId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Departments { get; set; }

        [Required(ErrorMessage = "يرجى اختيار المخزن.")]
        [Display(Name = "المخزن")]
        public Guid WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

        [Required(ErrorMessage = "يرجى اختيار حالة الرصيف.")]
        [Display(Name = "حالة الرصيف")]
        public Guid DockStatusId { get; set; }

        // Status DropDown
        public IEnumerable<SelectListItem>? DockStatus { get; set; }
    }
}