namespace Rassef.ViewModels.ExitTypes
{
    public class UpdateExitTypesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "اسم نوع الخروج")]
        public string Name { get; set; } = string.Empty;
    }
}
