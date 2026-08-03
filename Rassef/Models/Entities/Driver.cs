namespace Rassef.Models.Entities
{
    public class Driver : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int CreatedById { get; set; }
        public int DeiverTypeId { get; set;  }
        public DriverType DeiverType { get; set; }
        public User CreatedBy { get; set; }
        public ICollection<TransferRequest> TransferRequests { get; set; } = new HashSet<TransferRequest>();
        public ICollection<SupplierRequest> SupplierRequests { get; set; } = new HashSet<SupplierRequest>();
        
    }
}
