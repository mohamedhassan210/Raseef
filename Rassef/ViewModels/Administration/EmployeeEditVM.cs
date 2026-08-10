namespace Rassef.ViewModels.Administration
{
    public class EmployeeEditVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public string NationalId { get; set; } = string.Empty;

        public string UserCode { get; set; } = string.Empty;
    }
}