namespace Rassef.Models.Entities
{
    public class Truck : BaseEntity
    {
        public int TruckTypeId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string PlateLetter { get; set; } = string.Empty;
        public double StorageCapacity { get; set; }
        public bool IsRefrigerated { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public TruckTypes TruckType { get; set; }
        public ICollection<TransferRequest> TransferRequests { get; set; } = new HashSet<TransferRequest>();
        public ICollection<SupplierRequest> SupplierRequests { get; set; } = new HashSet<SupplierRequest>();
    }
}
