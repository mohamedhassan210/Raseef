namespace Rassef.Models.Common
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAT { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset UpdatedAT { get; set; } = DateTimeOffset.Now;
        public void MarkAsUpdated() => UpdatedAT = DateTimeOffset.Now;
        public bool IsDeleted { get; set; } = false;
    }
}
