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

        public string CreatedById { get; set; } = string.Empty;
        public User CreatedBy { get; set; }

        // Feature — الرصيف اللي المستخدم اختاره وقت إنشاء الطلب (اختياري)
        public int? DockId { get; set; }
        public Dock? Dock { get; set; }

        // Feature — الموظف اللي عمل "استدعاء الدور التالي" (Call Next) في
        // CallStation لهذا الطلب. بيتسجل تلقائياً وقت الاستدعاء، مش وقت
        // إنشاء الطلب.
        public int? CallerId { get; set; }
        public User? Caller { get; set; }

        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();

    }
}
