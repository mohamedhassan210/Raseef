namespace Rassef.ViewModels.Driver
{
    public class CreateDriverVM
    {
        [Required(ErrorMessage = "اسم السائق مطلوب.")]
        [Display(Name = "اسم السائق")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "يجب أن يكون اسم السائق بين 3 و100 حرف.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب.")]
        [Display(Name = "الرقم القومي")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "يجب أن يتكون الرقم القومي من 14 رقمًا.")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب.")]
        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string Phone { get; set; } = string.Empty;
    }
}