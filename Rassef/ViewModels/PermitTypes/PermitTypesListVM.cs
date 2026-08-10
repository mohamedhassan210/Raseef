namespace Rassef.ViewModels.PermitTypes
{
    public class PermitTypesListVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم نوع التصريح")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "طلبات النقل المرتبطة")]
        public int TransferRequestsCount { get; set; }

        [Display(Name = "طلبات الموردين المرتبطة")]
        public int SupplierRequestsCount { get; set; }
    }
}
