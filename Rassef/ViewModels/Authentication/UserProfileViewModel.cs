namespace Rassef.ViewModels.Authentication
{
    public class UserProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public Email Email { get; set; }

        public string Phone { get; set; } = string.Empty;
    }
}
