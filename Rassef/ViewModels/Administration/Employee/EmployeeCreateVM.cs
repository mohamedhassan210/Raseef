namespace Rassef.ViewModels.Administration.Employee
{
    public class EmployeeCreateVM
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public string NationalId { get; set; } = string.Empty;

        public string UserCode { get; set; } = string.Empty;
    }
}