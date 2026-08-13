namespace Rassef.ViewModels.Administration
{
    public class DriverEditVM
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string NationalId { get; set; } = string.Empty;

        public int DeiverTypeId { get; set; }
    }
}