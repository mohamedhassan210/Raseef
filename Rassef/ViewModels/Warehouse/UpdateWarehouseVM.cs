
namespace Rassef.ViewModels.Warehouse
{
    public class UpdateWarehouseVM
    {
        [Required]
        public Guid Id { get; set; } 

        [Display(Name = "Warehouse Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        public List<Guid> SelectedDepartmentIds { get; set; } = new List<Guid>();
        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}