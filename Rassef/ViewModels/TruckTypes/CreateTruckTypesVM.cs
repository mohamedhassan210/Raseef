namespace Rassef.ViewModels.TruckTypes
{
    public class CreateTruckTypesVM
    {
        [Display(Name = "الكود")]
        public int TruckTypeCode { get; set; }

        [Display(Name = "اسم نوع الشاحنة")]
        public string Name { get; set; } = string.Empty;
    }
}