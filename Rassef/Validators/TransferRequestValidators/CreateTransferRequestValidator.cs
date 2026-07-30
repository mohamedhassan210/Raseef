
namespace Rassef.Validators.TransferRequestValidators
{
    public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequestVM>
    {
        public CreateTransferRequestValidator()
        {
            RuleFor(x => x.AvizNumber)
                .NotEmpty().WithMessage("رقم الأفيز مطلوب.")
                .MaximumLength(50).WithMessage("رقم الأفيز لا يمكن أن يزيد عن 50 حرفًا.");

            RuleFor(x => x.TruckId)
                .GreaterThan(0).WithMessage("اختر عربة صحيحة.");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage("اختر سائقًا صحيحًا.");

            RuleFor(x => x.PermitTypeId)
                .GreaterThan(0).WithMessage("اختر نوع إذن صحيح.");

            RuleFor(x => x.PermitNumber)
                .NotEmpty().WithMessage("رقم الإذن مطلوب.")
                .MaximumLength(50).WithMessage("رقم الإذن لا يمكن أن يزيد عن 50 حرفًا.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("اختر قسمًا صحيحًا.");

            RuleFor(x => x.RequestStatusId)
                .GreaterThan(0).WithMessage("اختر حالة طلب صحيحة.");
        }
    }
}