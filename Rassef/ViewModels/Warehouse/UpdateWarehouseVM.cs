namespace Rassef.ViewModels.Warehouse
{
    public class UpdateWarehouseVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        // Same department+doc cards as Warehouses/Details — shown on the Edit
        // page too, per request. Not part of the posted form (managed through
        // their own add/remove actions), just populated for display on GET.
        public List<DepartmentCardVM> DepartmentCards { get; set; } = new List<DepartmentCardVM>();
    }
}