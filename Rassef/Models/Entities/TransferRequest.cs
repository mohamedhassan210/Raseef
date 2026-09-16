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

        // Feature — dock selection: last field on the Create form, chosen after
        // Department. Required to belong to DepartmentId and not be full/under
        // maintenance at the time of submission (server-side, see TransferRequestController).
        public int DockId { get; set; }
        public Dock Dock { get; set; }

        public int RequestStatusId { get; set; }
        public RequestStatuses RequestStatus { get; set; }

        public string CreatedById { get; set; } = string.Empty;
        public User CreatedBy { get; set; }

        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();

    }
}