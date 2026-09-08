namespace Rassef.ViewModels.DriverType
{
    public class DriverTypeDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "الكود")]
        public int Code { get; set; }

        [Display(Name = "اسم نوع السائق")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد السائقين المرتبطين")]
        public int DriversCount { get; set; }
    }
}