namespace Rassef.ViewModels.Authentication
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "User Name is required. ")]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "username")]
        public string userName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required. ")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [StringLength(100)]
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required. ")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your password. ")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
