namespace Rassef.Models.Entities
{
    public class TransferRequest : BaseEntity
    {
        public string AvizNumber { get; set; }
        public Guid TruckId { get; set; }
        public Guid DriverId { get; set; }
        public Guid PermitTypeId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid RequestStatusId { get; set; }
        public string DriverPhone { get; set; }
        public string PermitNumber { get; set; }
        public User CreatedBy { get; set; }

        public Truck Truck { get; set; }
        public Driver Driver { get; set; }
        public Department Department { get; set; }
        public Permit_types PermitType { get; set; }
        public Request_statuses RequestStatus { get; set; }
        public ICollection<QueueTicket> QueueTickets { get; set; }

    }
}
