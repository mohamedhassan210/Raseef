namespace Rassef.Models.Identity
{
    public class UserGroup : BaseEntity
    {
        public string  Name { get; set; }
        public ICollection<User> Users { get; set; }
        = new HashSet<User>();
        public ICollection<GroupPermission> GroupPermissions { get; set; } = new HashSet<GroupPermission>();
    }
}
