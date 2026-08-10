namespace Rassef.Models.StatusesAndActions
{
    public class ActionTypes : BaseEntity
    {
        public string Name { get;  set; } = string.Empty;
        public ICollection<QueueAction> QueueActions { get; set; } = new HashSet<QueueAction>();
    }
}
