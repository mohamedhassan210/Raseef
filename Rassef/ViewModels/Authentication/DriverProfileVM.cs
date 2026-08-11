namespace Rassef.ViewModels.Drivers
{
    public class DriverProfileVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم السائق")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "عدد الزيارات")]
        public int VisitsCount { get; set; }
    }
}