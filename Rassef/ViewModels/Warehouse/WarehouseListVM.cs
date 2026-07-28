
namespace Rassef.ViewModels.Warehouse
{
    public class WarehouseListVM
    {
        public Guid Id { get; set; }

        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "أنشئ بواسطة")]
        public string CreatedByName { get; set; } = string.Empty;

        [Display(Name = "عدد الأرصفة")]
        public int DocksCount { get; set; }

        [Display(Name = "عدد الأقسام")]
        public int DepartmentsCount { get; set; }
    }
}