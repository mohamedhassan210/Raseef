namespace Rassef.Models.Entities
{
    public class DockAssignment : BaseEntity
    {
        public Guid DockId { get; set; }
        public Guid TicketId { get; set; }
        public DateTimeOffset AssignedAt { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
        public User CreatedBy { get; set; }
        public Dock Dock { get; set; }
        public QueueTicket QueueTicket { get; set; }

    }
}
