using Rassef.ViewModels.Shared;

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
        [Display(Name = "الاختصار")]
        public string Prefix { get; set; } = string.Empty;

        // DropDown
        public IEnumerable<SelectListItem>? Warehouses { get; set; }

        // Same dock cards shown on Department/Details — displayed on the
        // Update page too, per request.
        public List<DockCardVM> Docks { get; set; } = new List<DockCardVM>();
    }
}