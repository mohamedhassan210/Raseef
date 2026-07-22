namespace Rassef.Models.Entities
{
    public class Driver : BaseEntity
    {
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public string Phone { get; set; }
        public string NationalCardPhoto { get; set; }
        public User CreatedBy { get; set; }
        public ICollection<TransferRequest> TransferRequests { get; set; }
        public ICollection<SupplierRequest> SupplierRequests { get; set; }

    }
}
