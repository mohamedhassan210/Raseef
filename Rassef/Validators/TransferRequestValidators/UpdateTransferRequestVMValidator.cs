namespace Rassef.Validators.TransferRequestValidators
{
    public class UpdateTransferRequestVMValidator : AbstractValidator<UpdateTransferRequestVM>
    {
        public UpdateTransferRequestVMValidator()
        {
            RuleFor(x => x.AvizNumber).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.TruckId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.DriverId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.PermitTypeId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.PermitNumber).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.RequestStatusId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
