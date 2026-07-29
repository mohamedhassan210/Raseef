namespace Rassef.ViewModels.Truck
{
    public class CreateTruckVM
    {
       
        [Display(Name = "رقم اللوحة")]
        public string PlateNumber { get; set; } = string.Empty;

        [Display(Name = "حروف اللوحة")]
        public string PlateLetter { get; set; } = string.Empty;

        
        [Display(Name = "السعة التخزينية")]
        public double StorageCapacity { get; set; }

        [Display(Name = "شاحنة مبردة")]
        public bool IsRefrigerated { get; set; }

    
        [Display(Name = "نوع الشاحنة")]
        public int TruckTypeId { get; set; }
    }
}