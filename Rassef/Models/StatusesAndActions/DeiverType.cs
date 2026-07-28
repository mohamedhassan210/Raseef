namespace Rassef.Models.StatusesAndActions
{
    public class DeiverType : BaseEntity
    {
        public Guid? Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
