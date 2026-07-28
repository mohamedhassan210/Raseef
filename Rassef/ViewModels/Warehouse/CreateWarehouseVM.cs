
namespace Rassef.ViewModels.Warehouse
{
    public class CreateWarehouseVM
    {
            [Display(Name = "Warehouse Name")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Location")]
            public string Location { get; set; } = string.Empty;

            public IEnumerable<SelectListItem>? Departments { get; set; }
            public IEnumerable<SelectListItem>? Docks { get; set; }

    }
}