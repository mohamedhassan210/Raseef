
namespace Rassef.ViewModels.Dock
{
    public class DockListVM
    {
        [Display(Name = "المعرف")]
        public Guid Id { get; set; }

        [Display(Name = "اسم الرصيف")]
        public string DockName { get; set; } = string.Empty;

        [Display(Name = "اسم المخزن")]
        public string WarehouseName { get; set; } = string.Empty;

        [Display(Name = "اسم القسم")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "حالة الرصيف")]
        public string DockStatusName { get; set; } = string.Empty;
    }
}