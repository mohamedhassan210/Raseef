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
        public string DriverNationalCardPhoto { get; set; }=string.Empty;
        public string DriverPhone { get; set; }=string.Empty;
        public string PermitNumber { get; set; }=string.Empty;
        public User CreatedBy { get; set; }
        public Supplier Supplier { get; set; }
        public Truck Truck { get; set; }
        public Driver Driver { get; set; }
        public Department Department { get; set; }
        public Permit_types PermitType { get; set; }
        public Commodity_types CommodityType { get; set; }
        public Request_statuses RequestStatus { get; set; }
        public ICollection<QueueTicket> QueueTickets { get; set; }=new HashSet<QueueTicket>();

    }
}
