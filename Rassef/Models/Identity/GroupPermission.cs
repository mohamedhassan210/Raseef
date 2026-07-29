namespace Rassef.Models.Identity
{
    public class GroupPermission : BaseEntity
    {
        public int GroupId { get; set; }
        public UserGroup? Group { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

    }
}
