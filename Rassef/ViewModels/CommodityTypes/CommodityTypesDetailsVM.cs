namespace Rassef.ViewModels.CommodityTypes
{
    public class CommodityTypesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "نوع السلعة")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد طلبات الموردين المرتبطة")]
        public int SupplierRequestsCount { get; set; }
    }
}
