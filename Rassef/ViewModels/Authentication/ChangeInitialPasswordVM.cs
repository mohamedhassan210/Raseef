using System.ComponentModel.DataAnnotations;

namespace Rassef.ViewModels.Authentication
{
    public class ChangeInitialPasswordVM
    {
        [Required(ErrorMessage = "يرجى إدخال كلمة المرور الحالية (كود الموظف).")]
        [Display(Name = "كلمة المرور الحالية (كود الموظف)")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى إدخال كلمة المرور الجديدة.")]
        [MinLength(6, ErrorMessage = "يجب ألا تقل كلمة المرور عن 6 خانات.")]
        [Display(Name = "كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى تأكيد كلمة المرور الجديدة.")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
