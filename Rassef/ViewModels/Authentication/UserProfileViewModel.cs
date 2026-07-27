namespace Rassef.ViewModels.Authentication
{
    public class UserProfileViewModel
    {
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

      
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

      
        [Display(Name = "Email Address")]
        public Email Email { get; set; } = default!;

    
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "National ID")]
        public string NationalId { get; set; } = string.Empty;
    }
}
