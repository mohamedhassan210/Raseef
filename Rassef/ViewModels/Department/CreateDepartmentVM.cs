namespace Rassef.ViewModels.Department
{
    public class CreateDepartmentVM
    {
        [Display(Name = "اسم القسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "المخزن")]
        public Guid WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }
    }
}