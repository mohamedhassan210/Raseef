namespace Rassef.ViewModels.Shared
{
    /// <summary>
    /// One card representing a Department, shown on Warehouses/Details and
    /// Warehouses/Edit. Carries the department's own Docks (ارصفة) so they
    /// render as nested mini-cards.
    /// </summary>
    public class DepartmentCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public List<DockCardVM> Docks { get; set; } = new List<DockCardVM>();
    }
}
