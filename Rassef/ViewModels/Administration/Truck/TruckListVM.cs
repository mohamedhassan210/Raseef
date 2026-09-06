namespace Rassef.ViewModels.Administration
{
    public class TruckListVM
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public string IsRefrigerated { get; set; } = string.Empty;

        public string TruckTypeName { get; set; } = string.Empty;

        public double StorageCapacity { get; set; }

        public string HostEmployeeName { get; set; } = string.Empty;
    }
}