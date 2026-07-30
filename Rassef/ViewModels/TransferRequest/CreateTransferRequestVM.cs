namespace Rassef.ViewModels.TransferRequest
{
    public class CreateTransferRequestVM
    {
        [Display(Name = "رقم الأفيز")]
        public string AvizNumber { get; set; } = string.Empty;

        [Display(Name = "العربة")]
        public int TruckId { get; set; }

        [Display(Name = "السائق")]
        public int DriverId { get; set; }

        [Display(Name = "نوع الإذن")]
        public int PermitTypeId { get; set; }

        [Display(Name = "رقم الإذن")]
        public string PermitNumber { get; set; } = string.Empty;

        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        [Display(Name = "حالة الطلب")]
        public int RequestStatusId { get; set; }
    }
}