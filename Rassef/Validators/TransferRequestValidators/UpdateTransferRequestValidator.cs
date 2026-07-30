
namespace Rassef.Validators.TransferRequestValidators
{
    public class UpdateTransferRequestValidator : AbstractValidator<UpdateTransferRequestVM>
    {
        public UpdateTransferRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف الطلب غير صحيح.");

            RuleFor(x => x.AvizNumber)
                .NotEmpty().WithMessage("رقم الأفيز مطلوب.")
                .MaximumLength(50).WithMessage("رقم الأفيز لا يمكن أن يزيد عن 50 حرفًا.");

            RuleFor(x => x.TruckId)
                .GreaterThan(0).WithMessage("يجب اختيار عربة.");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage("يجب اختيار سائق.");

            RuleFor(x => x.PermitTypeId)
                .GreaterThan(0).WithMessage("يجب اختيار نوع الإذن.");

            RuleFor(x => x.PermitNumber)
                .NotEmpty().WithMessage("رقم الإذن مطلوب.")
                .MaximumLength(50).WithMessage("رقم الإذن لا يمكن أن يزيد عن 50 حرفًا.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("يجب اختيار القسم.");

            RuleFor(x => x.RequestStatusId)
                .GreaterThan(0).WithMessage("يجب اختيار حالة الطلب.");
        }
    }
}