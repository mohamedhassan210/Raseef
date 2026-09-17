namespace Rassef.ViewModels.Shared
{
    /// <summary>
    /// One small card representing a SupplierRequest ("doc") nested inside a
    /// department card (Warehouse Details/Edit) or shown directly on the
    /// Department Details/Update page.
    /// </summary>
    public class DocCardVM
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string TruckInfo { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
    }
}
