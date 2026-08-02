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
        public string? EmpCode { get; set; }
        public string BranchCode { get; set; }
        public int PositionId { get; set; }
        public Position Position { get; set; }
        public int GroupId { get; set; }
        public UserGroup Group { get; set; }
    }
}
