namespace Rassef.Models.StatusesAndActions
{
    public class DockStatuses : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Dock> Docks { get; set; } = new HashSet<Dock>();
    }
}
