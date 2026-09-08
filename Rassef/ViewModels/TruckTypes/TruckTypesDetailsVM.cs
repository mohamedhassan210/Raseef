namespace Rassef.ViewModels.TruckTypes
{
    public class TruckTypesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "الكود")]
        public int TruckTypeCode { get; set; }

        [Display(Name = "اسم نوع الشاحنة")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد الشاحنات المرتبطة")]
        public int TrucksCount { get; set; }
    }
}