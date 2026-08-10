    namespace Rassef.ViewModels.DockStatuses
    {
        public class UpdateDockStatusesVM
        {
            [Required]
            public int Id { get; set; }

            [Display(Name = "اسم حالة الرصيف")]
            public string Name { get; set; } = string.Empty;
        }
    }
