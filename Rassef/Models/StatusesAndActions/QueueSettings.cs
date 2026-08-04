namespace Rassef.Models.Entities
{
    public class QueueSettings : BaseEntity
    {
        public ResetType ResetType { get; set; }

        public int? ShiftId { get; set; }

        public Shift? Shift { get; set; }
    }
}