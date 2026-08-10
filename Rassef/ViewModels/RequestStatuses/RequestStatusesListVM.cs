namespace Rassef.ViewModels.RequestStatuses
{
    public class RequestStatusesListVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم حالة الطلب")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "طلبات النقل المرتبطة")]
        public int TransferRequestsCount { get; set; }

        [Display(Name = "طلبات الموردين المرتبطة")]
        public int SupplierRequestsCount { get; set; }
    }
}
