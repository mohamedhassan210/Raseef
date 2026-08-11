
namespace Rassef.ViewModels.Supplier
{
    public class SupplierListVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;       // من Supplier.Name
        public string Phone { get; set; } = string.Empty;      // من Supplier.Phone
        public string SupCode { get; set; } = string.Empty;    // من Supplier.SupCode
        public string LogoURL { get; set; } = string.Empty;    // من Supplier.LogoURL
        public string HostEmployeeName { get; set; } = string.Empty; // من Supplier.CreatedBy.Name
    }
}