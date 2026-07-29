namespace Rassef.Models.Identity
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Email? Email { get; set; }
        public string HashPassword { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NationalId { get; set; }
        public string? Code { get; set; }

        public ICollection<UserGroup> Groups { get; set; }
    }
}
