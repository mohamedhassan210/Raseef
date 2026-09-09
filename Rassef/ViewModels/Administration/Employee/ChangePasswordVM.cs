namespace Rassef.ViewModels.Administration.Employee
{
    public class ChangePasswordVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم الموظف")]
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}