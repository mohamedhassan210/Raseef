

namespace Rassef.ViewModels.Warehouse
{
    public class CreateWarehouseVM
    {
        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "الأقسام")]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();

        [Display(Name = "الأرصفة")]
        public List<int> SelectedDockIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Departments { get; set; }
        public IEnumerable<SelectListItem>? Docks { get; set; }
    }
}