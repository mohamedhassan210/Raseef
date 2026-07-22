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
    }
}
