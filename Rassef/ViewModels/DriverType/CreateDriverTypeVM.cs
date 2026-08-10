namespace Rassef.ViewModels.DriverType
{
    public class CreateDriverTypeVM
    {
        [Display(Name = "كود نوع السائق")]
        public int? Code { get; set; }

        [Display(Name = "اسم نوع السائق")]
        public string Name { get; set; } = string.Empty;
    }
}
