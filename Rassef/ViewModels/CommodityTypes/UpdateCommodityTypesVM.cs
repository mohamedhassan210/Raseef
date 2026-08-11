namespace Rassef.ViewModels.CommodityTypes
{
    public class UpdateCommodityTypesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "نوع السلعة")]
        public string Name { get; set; } = string.Empty;
    }
}
