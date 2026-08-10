        namespace Rassef.ViewModels.CommodityTypes
        {
            public class CreateCommodityTypesVM
            {
                [Required(ErrorMessage = "يرجى إدخال اسم نوع السلعة.")]
                [StringLength(100, ErrorMessage = "اسم نوع السلعة يجب ألا يتجاوز 100 حرف.")]
                [Display(Name = "نوع السلعة")]
                public string Name { get; set; } = string.Empty;
            }
        }
