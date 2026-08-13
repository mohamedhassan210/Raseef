namespace Rassef.ViewModels.Truck
{
    public class TruckListVM
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public string PlateLetter { get; set; } = string.Empty;

        public double StorageCapacity { get; set; }

        public bool IsRefrigerated { get; set; }

        public int supplierId { get; set; }

        public string TruckTypeName { get; set; } = string.Empty;
    }
}