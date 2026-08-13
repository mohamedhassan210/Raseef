

namespace Rassef.ViewModels.TransferRequest
{
    public class CreateTransferRequestVM
    {
        // 1. تحديد القسم (Dropdown)
        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; } = Enumerable.Empty<SelectListItem>();


        // 2. نوع الإذن (Radio Button: خروج / دخول)
        [Display(Name = "نوع الإذن")]
        public int PermitTypeId { get; set; }

        public IEnumerable<SelectListItem> PermitTypes { get; set; } = Enumerable.Empty<SelectListItem>();


        // 3. رقم الإذن (Input Text)
        [Display(Name = "رقم الإذن")]
        public string PermitNumber { get; set; } = string.Empty;


        // 4. رقم الأفيز (Input Text)
        [Display(Name = "رقم الأفيز")]
        public string AvizNumber { get; set; } = string.Empty;

    }
}