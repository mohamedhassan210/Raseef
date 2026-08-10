    namespace Rassef.ViewModels.PermitTypes
    {
        public class UpdatePermitTypesVM
        {
            [Required]
            public int Id { get; set; }

            [Display(Name = "اسم نوع التصريح")]
            public string Name { get; set; } = string.Empty;
        }
    }
