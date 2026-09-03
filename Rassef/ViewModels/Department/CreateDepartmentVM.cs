namespace Rassef.ViewModels.Department
{
    public class CreateDepartmentVM
    {
        [Display(Name = "اسم القسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الاختصار")]
        public string Prefix { get; set; } = string.Empty;

        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }

        [Display(Name = "نوع القسم")]
        public int DepartmentTypeId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

        // DropDown — Q5
        public IEnumerable<SelectListItem>? DepartmentTypes { get; set; }
    }
}