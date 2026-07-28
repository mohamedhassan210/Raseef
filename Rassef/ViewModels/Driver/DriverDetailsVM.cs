namespace Rassef.ViewModels.Driver
{
    public class DriverDetailsVM
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string NationalId { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; } 
    }
}