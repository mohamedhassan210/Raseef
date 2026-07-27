namespace Rassef.ViewModels.Authentication
{
    public class ForgetPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public Email Email { get; set; }

    }
}
