namespace Rassef.Models.Entities
{
    public class DockAssignment : BaseEntity
    {
        public int DockId { get; set; }
        public int TicketId { get; set; }
        public DateTimeOffset AssignedAt { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
        public int CreatedBy { get; set; }


    }
}
