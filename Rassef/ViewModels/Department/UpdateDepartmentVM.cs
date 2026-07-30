using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Rassef.ViewModels.Department
{
    public class UpdateDepartmentVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "اسم القسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }
    }
}