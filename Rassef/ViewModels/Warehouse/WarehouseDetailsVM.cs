using Rassef.ViewModels.Shared;

namespace Rassef.ViewModels.Warehouse
{
    public class WarehouseDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم المخزن")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "تم الإنشاء بواسطة")]
        public string CreatedByName { get; set; } = string.Empty;

        // Card-based departments+docs display — replaces the old AllDepartments
        // "linked/not linked" list. Each department belongs to exactly one
        // warehouse (WarehouseId is a required FK), so this is simply
        // warehouse.Departments rendered as cards, each carrying its own docs
        // (SupplierRequests) as nested mini-cards.
        public List<DepartmentCardVM> DepartmentCards { get; set; } = new List<DepartmentCardVM>();

        [Display(Name = "الأرصفة")]
        public List<WarehouseLinkedItemVM> AllDocks { get; set; } = new List<WarehouseLinkedItemVM>();
    }

    /// <summary>
    /// Read-only row for the Details page: a department/dock name plus whether
    /// it's linked to the warehouse being viewed. Not used for selection/editing.
    /// </summary>
    public class WarehouseLinkedItemVM
    {
        public string Name { get; set; } = string.Empty;
        public bool IsLinked { get; set; }
    }
}