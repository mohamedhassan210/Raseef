namespace Rassef.ViewModels.Shared
{
    /// <summary>
    /// One card representing a Department, shown on Warehouses/Details and
    /// Warehouses/Edit. Carries the "docs" (SupplierRequests) that belong to
    /// it so they render as nested mini-cards.
    /// </summary>
    public class DepartmentCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public List<DocCardVM> Docs { get; set; } = new List<DocCardVM>();
    }
}
