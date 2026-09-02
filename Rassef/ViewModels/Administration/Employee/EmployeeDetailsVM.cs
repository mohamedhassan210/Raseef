namespace Rassef.ViewModels.Administration.Employee
{
    public class EmployeeDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }
}