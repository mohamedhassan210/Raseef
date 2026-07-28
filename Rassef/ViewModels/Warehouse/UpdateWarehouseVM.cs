
namespace Rassef.ViewModels.Warehouse
{
    public class UpdateWarehouseVM
    {
        [Required(ErrorMessage = "معرف المخزن مطلوب.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "اسم المخزن مطلوب.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "يجب أن يكون اسم المخزن بين 2 و 100 حرف.")]
        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "الموقع مطلوب.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "يجب أن يكون الموقع بين 3 و 200 حرف.")]
        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "الأقسام المحددّة")]
        public List<Guid> SelectedDepartmentIds { get; set; } = new List<Guid>();

        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}