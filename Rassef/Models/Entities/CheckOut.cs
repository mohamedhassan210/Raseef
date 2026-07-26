namespace Rassef.Models.Entities
{
    public class CheckOut : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid ExitTypeId { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public User CreatedBy { get; set; } = null!;
        public QueueTicket QueueTicket { get; set; }
        public Exit_types ExitType { get; set; }
    }
}
