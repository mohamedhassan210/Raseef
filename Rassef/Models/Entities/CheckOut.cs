namespace Rassef.Models.Entities
{
    public class CheckOut : BaseEntity
    {
        public int TicketId { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public User CreatedBy { get; set; } = null!;
        public QueueTicket QueueTicket { get; set; }
        public int ExitTypeId { get; set; }
        public ExitTypes ExitType { get; set; }
    }
}
