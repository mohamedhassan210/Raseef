
namespace Rassef.Models.Entities
{
    public class QueueTicket : BaseEntity
    {
        public int TicketNumber { get; set; }
        public int TransferRequestId { get; set; }
        public int SupplierRequestId { get; set; }
        public int DepartmentId { get; set; }
        public int TicketStatusId { get; set; }
        public DateTimeOffset QueueTime { get; set; }
        public DateTimeOffset EntryTime { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public User CreatedBy { get; set; }
        public TransferRequest? TransferRequest { get; set; }
        public SupplierRequest? SupplierRequest { get; set; }
        public Department Department { get; set; }
        public Ticket_statuses TicketStatus { get; set; }
        public CheckOut CheckOut { get; set; }
        public ICollection<DockAssignment> DockAssignments { get; set; }
        public ICollection<QueueAction> QueueActions { get; set; }

    }
}
