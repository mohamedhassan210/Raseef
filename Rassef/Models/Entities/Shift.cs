namespace Rassef.Models.Entities
{
    public class Shift : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public TimeSpan Duration { get; set; }
        public ICollection<QueueSettings> QueueSettings { get; set; }
            = new HashSet<QueueSettings>();

        public ICollection<QueueTicket> QueueTickets { get; set; }
            = new HashSet<QueueTicket>();
    }
}