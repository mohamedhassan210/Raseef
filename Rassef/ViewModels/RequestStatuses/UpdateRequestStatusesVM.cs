namespace Rassef.ViewModels.RequestStatuses
{
    public class UpdateRequestStatusesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "اسم حالة الطلب")]
        public string Name { get; set; } = string.Empty;
    }
}
