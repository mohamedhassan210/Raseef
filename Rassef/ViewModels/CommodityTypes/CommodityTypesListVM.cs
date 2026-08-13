namespace Rassef.ViewModels.CommodityTypes
{
    public class CommodityTypesListVM
    {
        public int Id { get; set; }

        [Display(Name = "نوع السلعة")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد طلبات الموردين")]
        public int SupplierRequestsCount { get; set; }
    }
}
