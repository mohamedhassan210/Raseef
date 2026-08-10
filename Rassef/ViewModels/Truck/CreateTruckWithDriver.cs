namespace Rassef.Models.ViewModels.Truck
{
    public class TruckWithDriverVM
    {
        [Display(Name = "نوع الشاحنة")]
        public int TruckTypeId { get; set; }

        [Display(Name = "رقم الشاحنة")]
        public string PlateNumber { get; set; } = string.Empty;

        [Display(Name = "حروف الشاحنة")]
        public string PlateLetter { get; set; } = string.Empty;

        [Display(Name = "سعة التخزين")]
        public double StorageCapacity { get; set; }

        [Display(Name = "شاحنة مبردة")]
        public bool IsRefrigerated { get; set; }

        [Display(Name = "السائق")]
        public int DriverId { get; set; }
        public int SupId { get; set; }
        public IEnumerable<SelectListItem> Drivers { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> TruckTypes { get; set; }
            = new List<SelectListItem>();
    }
}