

namespace Rassef.Validators.TransferRequest
{
    public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequestVM>
    {
        public CreateTransferRequestValidator()
        {
            // 1. التحقق من اختيار القسم
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("يرجى اختيار القسم.")
                .GreaterThan(0).WithMessage("يرجى اختيار قسم صحيح.");

            // 2. التحقق من اختيار نوع الإذن
            RuleFor(x => x.PermitTypeId)
                .NotEmpty().WithMessage("يرجى تحديد نوع الإذن.")
                .GreaterThan(0).WithMessage("يرجى تحديد نوع إذن صحيح.");

            // 3. التحقق من رقم الإذن
            RuleFor(x => x.PermitNumber)
                .NotEmpty().WithMessage("يرجى إدخال رقم الإذن.")
                .MaximumLength(50).WithMessage("رقم الإذن يجب ألا يتجاوز 50 حرفاً/رقماً.");

            // 4. التحقق من رقم الأفيز
            RuleFor(x => x.AvizNumber)
                .NotEmpty().WithMessage("يرجى إدخال رقم الأفيز.")
                .MaximumLength(50).WithMessage("رقم الأفيز يجب ألا يتجاوز 50 حرفاً/رقماً.");

        }
    }
}