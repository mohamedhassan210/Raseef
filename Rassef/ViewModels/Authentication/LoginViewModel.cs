namespace Rassef.ViewModels.Authentication
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please confirm your password. ")]
        [Display(Name ="Username or Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage ="Passowrd is required. ")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name ="Remember me")]
        public bool RememberMe { get; set; }
    }
}
