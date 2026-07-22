using Rassef.Models.Common;

namespace Rassef.Models.Entities
{
    public class QueueAction : BaseEntity
    {
        public int TicketId { get; set; }
        public int ActionTypeId { get; set; }
        public DateTimeOffset ActionTime { get; set; }

    }
}
