namespace Rassef.Models.StatusesAndActions
{
    public class TicketStatuses : BaseEntity
    {
        public string Name { get; protected set; } = string.Empty;
        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();
    }
}
