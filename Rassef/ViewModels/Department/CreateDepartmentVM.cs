using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rassef.ViewModels.Department
{
    public class CreateDepartmentVM
    {
        [Required]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Warehouse")]
        public Guid WarehouseId { get; set; }

        //DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

    }
}
