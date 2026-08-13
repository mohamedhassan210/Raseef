namespace Rassef.ViewModels.Authentication
{
    public class LoginViewModel
    {
        public string UserNameOrEmail { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
