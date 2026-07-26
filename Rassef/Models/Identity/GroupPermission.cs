namespace Rassef.Models.Identity
{
    public class GroupPermission
    {
        public Guid GroupId { get; set; }
        public GroupUser? Group { get; set; }

        public Guid PermissionId { get; set; }
        public Permissions? Permission { get; set; }

    }
}
