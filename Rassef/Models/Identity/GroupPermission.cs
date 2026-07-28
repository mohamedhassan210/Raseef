namespace Rassef.Models.Identity
{
    public class GroupPermission
    {
        public Guid GroupId { get; set; }
        public UserGroup? Group { get; set; }

        public Guid PermissionId { get; set; }
        public Permission? Permission { get; set; }

    }
}
    