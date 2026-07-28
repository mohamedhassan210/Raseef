namespace Rassef.ViewModels.Truck
{
    public class TruckDetailsVM
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public string PlateLetter { get; set; } = string.Empty;

        public double StorageCapacity { get; set; }

        public bool IsRefrigerated { get; set; }

        public string TruckTypeName { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}