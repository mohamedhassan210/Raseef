namespace Rassef.Models.Identity
{
    public class Permission : BaseEntity
    {
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public ICollection<GroupPermission> GroupPermissions { get; set; }
            = new HashSet<GroupPermission>();
    }
}
