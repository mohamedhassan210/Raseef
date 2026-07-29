
namespace Rassef.ViewModels.Warehouse
{
    public class WarehouseDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "تم الإنشاء بواسطة")]
        public string CreatedByName { get; set; } = string.Empty;

        [Display(Name = "الأقسام المرتبطة")]
        public List<string> DepartmentNames { get; set; } = new List<string>();

        [Display(Name = "الأرصفة المرتبطة")]
        public List<string> DockNames { get; set; } = new List<string>();
    }
}