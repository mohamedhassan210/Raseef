namespace Rassef.Models.Entities
{
    public class TransferRequest : BaseEntity
    {
        public string AvizNumber { get; set; } = string.Empty;
        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }

        public int PermitTypeId { get; set; }
        public PermitTypes PermitType { get; set; }

        public string PermitNumber { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int RequestStatusId { get; set; }
        public RequestStatuses RequestStatus { get; set; }

        public User CreatedBy { get; set; }

        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();

    }
}
