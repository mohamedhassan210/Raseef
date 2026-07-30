

namespace Rassef.ViewModels.Warehouse
{
    public class UpdateWarehouseVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "الأقسام المحددّة")]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}