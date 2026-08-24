namespace Rassef.ViewModels.Administration
{
    public class SupplierDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SupCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? LogoURL { get; set; }
        public string HostEmployeeName { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
    }
}
