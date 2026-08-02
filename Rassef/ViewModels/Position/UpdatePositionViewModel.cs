namespace Rassef.ViewModels.Position
{
    public class UpdatePositionViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "كود المنصب مطلوب.")]
        [Range(1, int.MaxValue, ErrorMessage = "كود المنصب يجب أن يكون أكبر من صفر.")]
        [Display(Name = "كود المنصب")]
        public int PositionCode { get; set; }

        [Required(ErrorMessage = "اسم المنصب مطلوب.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم المنصب يجب أن يكون بين 2 و100 حرف.")]
        [Display(Name = "اسم المنصب")]
        public string PositionName { get; set; } = string.Empty;
    }
}