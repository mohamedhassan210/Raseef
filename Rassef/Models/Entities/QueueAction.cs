namespace Rassef.Models.Entities
{
    public class QueueAction : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid ActionTypeId { get; set; }
        public DateTimeOffset ActionTime { get; set; }
        public QueueTicket QueueTicket { get; set; }
        public Action_types ActionType { get; set; }
    }
}
