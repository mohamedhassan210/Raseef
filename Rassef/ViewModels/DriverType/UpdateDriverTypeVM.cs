namespace Rassef.ViewModels.DriverType
{
    public class UpdateDriverTypeVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "الكود")]
        public int Code { get; set; }

        [Display(Name = "اسم نوع السائق")]
        public string Name { get; set; } = string.Empty;
    }
}