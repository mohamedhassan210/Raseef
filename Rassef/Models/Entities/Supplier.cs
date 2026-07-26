namespace Rassef.Models.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public User CreatedBy { get; set; }
        public string LogoURL {  get; set; }
        public ICollection<SupplierRequest> SupplierRequests { get; set; }

    }
}
