namespace Rassef.ViewModels.Administration.Truck
{
    public class TruckDetailsVM
    {
        public int Id { get; set; }
        public int VisitsCount { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string IsRefrigerated { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public double StorageCapacity { get; set; }
        public string HostEmployeeName { get; set; } = string.Empty;
    }
}