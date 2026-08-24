namespace Rassef.ViewModels.Group
{
    public class GroupDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = "إدارة المجموعات والعمليات";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int UsersCount => Employees.Count;
        public List<GroupEmployeeVM> Employees { get; set; } = new();
    }

    public class GroupEmployeeVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
