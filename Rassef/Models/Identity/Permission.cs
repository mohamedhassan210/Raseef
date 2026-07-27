namespace Rassef.Models.Identity
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<GroupPermission> GroupPermissions { get; set; }
            = new HashSet<GroupPermission>();
    }
}
