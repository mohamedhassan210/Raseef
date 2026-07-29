namespace Rassef.Models.Identity
{
    public class GroupPermission
    {
        public int GroupId { get; set; }
        public UserGroup? Group { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

    }
}
    