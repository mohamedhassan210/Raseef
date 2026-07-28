using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Rassef.ViewModels.Department
{
    public class UpdateDepartmentVM
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [DisplayName("Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Warehouse")]
        public Guid WarehouseId { get; set; }
        //DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }
    }
}
