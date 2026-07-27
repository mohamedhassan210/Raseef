namespace Rassef.Models.Identity
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Guid GroupId { get; set; }
        public UserGroup group { get; set; }
    }
}
