namespace Rassef.ViewModels.TransferRequest
{
    public class FinishRequestViewModel
    {
        [Display(Name = "رقم الإذن")]
        public string PermitNumber { get; set; } = string.Empty;

        [Display(Name = "رقم الأفيز")]
        public string AvizNumber { get; set; } = string.Empty;

        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        // ده هيشيل الـ PermitType المختار
        public int PermitTypeId { get; set; }

        // اللي هنملى بيها الـ Radio Buttons
        public List<SelectListItem> PermitTypes { get; set; } = new();
    }
}
