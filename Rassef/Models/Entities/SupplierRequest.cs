namespace Rassef.Models.Entities
{
    public class SupplierRequest : BaseEntity
    {
        public int SupplierId { get; set; }
        public int TruckId { get; set; }
        public int DriverId { get; set; }
        public int DepartmentId { get; set; }
        public int PermitTypeId { get; set; }
        public int CommodityTypeId { get; set; }
        public int RequestStatusId { get; set; }
        public string DriverNationalCardPhoto { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public string PermitNumber { get; set; } = string.Empty;
        public bool IsFood { get; set; }
        public User CreatedBy { get; set; }
        public Supplier Supplier { get; set; }
        public Truck Truck { get; set; }
        public Driver Driver { get; set; }
        public Department Department { get; set; }
        public PermitTypes PermitType { get; set; }
        public CommodityTypes CommodityType { get; set; }
        public RequestStatuses RequestStatus { get; set; }
        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();

    }
}
