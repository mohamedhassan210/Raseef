namespace Rassef.ViewModels.Authentication
{
    public class RegisterViewModel
    {
       
       
            [Display(Name = "Username")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
    }
}
