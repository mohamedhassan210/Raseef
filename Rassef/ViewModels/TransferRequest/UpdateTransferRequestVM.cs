namespace Rassef.ViewModels.TransferRequest
{
    public class UpdateTransferRequestVM
    {
        [Required(ErrorMessage = "معرف الطلب مطلوب.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الأفيز مطلوب.")]
        [Display(Name = "رقم الأفيز")]
        [StringLength(50, ErrorMessage = "رقم الأفيز لا يمكن أن يزيد عن 50 حرفًا.")]
        public string AvizNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اختر العربة.")]
        [Display(Name = "العربة")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار عربة.")]
        public int TruckId { get; set; }

        [Required(ErrorMessage = "اختر السائق.")]
        [Display(Name = "السائق")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار سائق.")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "اختر نوع الإذن.")]
        [Display(Name = "نوع الإذن")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار نوع الإذن.")]
        public int PermitTypeId { get; set; }

        [Required(ErrorMessage = "رقم الإذن مطلوب.")]
        [Display(Name = "رقم الإذن")]
        [StringLength(50, ErrorMessage = "رقم الإذن لا يمكن أن يزيد عن 50 حرفًا.")]
        public string PermitNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اختر القسم.")]
        [Display(Name = "القسم")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار القسم.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "اختر حالة الطلب.")]
        [Display(Name = "حالة الطلب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار حالة الطلب.")]
        public int RequestStatusId { get; set; }
    }
}