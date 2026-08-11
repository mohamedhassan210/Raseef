namespace Rassef.Models.StatusesAndActions
{
    public class TicketStatuses : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<QueueTicket> QueueTickets { get; set; } = new HashSet<QueueTicket>();
    }
}
