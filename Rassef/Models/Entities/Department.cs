namespace Rassef.Models.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public int WarehouseId { get; set; }
        public User CreatedBy { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<Dock> Docks { get; set; }
        public ICollection<QueueTicket> QueueTickets { get; set; }
        public ICollection<TransferRequest> TransferRequests { get; set; }
        public ICollection<SupplierRequest> SupplierRequests { get; set; }

    }
}
