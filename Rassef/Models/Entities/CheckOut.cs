using Rassef.Models.Common;

namespace Rassef.Models.Entities
{
    public class CheckOut : BaseEntity
    {
        public int TicketId { get; set; }
        public int ExitTypeId { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public int CreatedBy { get; set; }


    }
}
