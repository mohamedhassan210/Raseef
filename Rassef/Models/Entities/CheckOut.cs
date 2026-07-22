namespace Rassef.Models.Entities
{
    public class CheckOut : BaseEntity
    {
        public int TicketId { get; set; }
        public int ExitTypeId { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public User CreatedBy { get; set; }

        public QueueTicket QueueTicket { get; set; }
        public Exit_types ExitType { get; set; }
    }
}
