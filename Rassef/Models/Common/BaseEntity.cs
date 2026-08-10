namespace Rassef.Models.Common
{
    public class BaseEntity
    {
        public int Id { get; protected set; }
        public DateTimeOffset CreatedAT { get; protected set; }
        public DateTimeOffset UpdatedAT { get; protected set; }
        public void MarkAsUpdated() => UpdatedAT = DateTimeOffset.Now;
        public bool IsDeleted { get; set; } = false;    

    }
}
