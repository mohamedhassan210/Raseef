namespace Rassef.ViewModels.Warehouse
{
    public class CreateWarehouseVM
    {
        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;
    }
}