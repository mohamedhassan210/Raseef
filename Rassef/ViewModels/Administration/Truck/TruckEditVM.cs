namespace Rassef.ViewModels.Administration
{
    public class TruckEditVM
    {
        public int Id { get; set; }

        public string PlateLetter { get; set; } = string.Empty;

        public string PlateNumber { get; set; } = string.Empty;

        public double StorageCapacity { get; set; }

        public bool IsRefrigerated { get; set; }

        public int TruckTypeId { get; set; }
    }
}