namespace Rassef.Models.Entities
{
    public class Truck : BaseEntity
    {
        public string PlateNumber { get; set; }
        public Guid TruckTypeId { get; set; }
        public float StorageCapacity { get; set; }
        public bool IsRefrigerated { get; set; }
        public bool IsFood { get; set; }
        public User CreatedBy { get; set; }
        public Truck_types TruckType { get; set; }
        public ICollection<TransferRequest> TransferRequests { get; set; }
        public ICollection<SupplierRequest> SupplierRequests { get; set; }

    }
}
