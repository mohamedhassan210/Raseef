namespace Rassef.ViewModels.Authentication
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [Display(Name = "Email Address")]
        public Email Email { get; set; } = default!;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "National ID is required.")]
        [StringLength(14, MinimumLength = 14,
            ErrorMessage = "National ID must be exactly 14 digits.")]
        [RegularExpression(@"^\d{14}$",
            ErrorMessage = "National ID must contain exactly 14 digits.")]
        [Display(Name = "National ID")]
        public string NationalId { get; set; } = string.Empty;
    }
}
