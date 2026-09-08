namespace Rassef.ViewModels.TruckTypes
{
    public class UpdateTruckTypesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "الكود")]
        public int TruckTypeCode { get; set; }

        [Display(Name = "اسم نوع الشاحنة")]
        public string Name { get; set; } = string.Empty;
    }
}