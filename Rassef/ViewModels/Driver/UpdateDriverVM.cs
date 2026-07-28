namespace Rassef.ViewModels.Driver
{
    public class UpdateDriverVM
    {
        [Required]
        public Guid Id { get; set; }

    
        [Display(Name = "اسم السائق")]
    
        public string FullName { get; set; } = string.Empty;

       
        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;
    }
}