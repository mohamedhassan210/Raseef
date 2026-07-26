namespace Rassef.Models.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }= string.Empty;
        public int WarehouseId { get; set; }
        public User CreatedBy { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<Dock> Docks { get; set; }= new HashSet<Dock>();
        public ICollection<QueueTicket> QueueTickets { get; set; }=new HashSet<QueueTicket>();
        public ICollection<TransferRequest> TransferRequests { get; set; }=new HashSet<TransferRequest>();
        public ICollection<SupplierRequest> SupplierRequests { get; set; }=new HashSet<SupplierRequest>();

    }
}
