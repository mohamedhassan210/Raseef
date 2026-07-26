namespace Rassef.Models.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }=string.Empty;
        public string Phone { get; set; } = string.Empty;
        public User CreatedBy { get; set; }
        public string LogoURL { get; set; } = string.Empty;
        public ICollection<SupplierRequest> SupplierRequests { get; set; } = new HashSet<SupplierRequest>();

    }
}
