namespace Rassef.ViewModels.Shared
{
    /// <summary>
    /// One "dock" (رصيف) mini-card, nested inside a department card on
    /// Warehouses/Details, Warehouses/Edit, Department/Details and
    /// Department/Update. Represents an actual Dock entity that belongs
    /// to the department — not a document/SupplierRequest.
    /// </summary>
    public class DockCardVM
    {
        public int Id { get; set; }
        public string DockName { get; set; } = string.Empty;
        public string DockStatusName { get; set; } = string.Empty;
    }
}
