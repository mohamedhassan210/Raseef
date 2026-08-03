namespace Rassef.Models.StatusesAndActions
{
    public class ActionTypes : BaseEntity
    {
        public string Name { get; protected set; } = string.Empty;
        public ICollection<QueueAction> QueueActions { get; set; } = new HashSet<QueueAction>();
    }
}
