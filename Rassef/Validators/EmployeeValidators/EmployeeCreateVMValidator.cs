using FluentValidation;
using Rassef.ViewModels.Administration.Employee;

namespace Rassef.Validators.EmployeeValidators
{
    public class EmployeeCreateVMValidator : AbstractValidator<EmployeeCreateVM>
    {
        public EmployeeCreateVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الموظف مطلوب.")
                .MaximumLength(100).WithMessage("يجب ألا يتجاوز اسم الموظف 100 حرف.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^(010|011|012|015)\d{8}$")
                .WithMessage("رقم الهاتف غير صالح (يجب أن يكون رقم مصري مكون من 11 رقماً يبدأ بـ 010 أو 011 أو 012 أو 015).");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("يرجى إدخال بريد إلكتروني صالح.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.")
                .Length(14).WithMessage("الرقم القومي يجب أن يتكون من 14 رقماً بالضبط.")
                .Matches(@"^\d{14}$").WithMessage("الرقم القومي يجب أن يحتوي على أرقام فقط.");

            RuleFor(x => x.PositionId)
                .GreaterThan(0).WithMessage("يرجى اختيار الدور الوظيفي للموظف.");

            RuleFor(x => x.GroupId)
                .GreaterThan(0).WithMessage("يرجى اختيار مجموعة الصلاحيات للموظف.");

            RuleFor(x => x.UserCode)
                .NotEmpty().WithMessage("كود الموظف مطلوب.")
                .MaximumLength(50).WithMessage("كود الموظف يجب ألا يتجاوز 50 حرفاً.");

            RuleFor(x => x.BranchCode)
                .NotEmpty().WithMessage("كود الفرع مطلوب.")
                .MaximumLength(50).WithMessage("كود الفرع يجب ألا يتجاوز 50 حرفاً.");
        }
    }
}
