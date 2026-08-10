namespace Rassef.ViewModels.RequestStatuses
{
    public class RequestStatusesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم حالة الطلب")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "إجمالي عدد طلبات النقل")]
        public int TransferRequestsCount { get; set; }

        [Display(Name = "إجمالي عدد طلبات الموردين")]
        public int SupplierRequestsCount { get; set; }
    }
}
