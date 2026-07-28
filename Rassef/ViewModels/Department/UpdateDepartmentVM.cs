using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Rassef.ViewModels.Department
{
    public class UpdateDepartmentVM
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب.")]
        [Display(Name = "اسم القسم")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "يجب أن يكون اسم القسم بين 3 و100 حرف.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى اختيار المخزن.")]
        [Display(Name = "المخزن")]
        public Guid WarehouseId { get; set; }

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }
    }
}