namespace Rassef.Models.Identity
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Email? Email { get; set; }
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
        public UserGroup group { get; set; } 
    }
}
