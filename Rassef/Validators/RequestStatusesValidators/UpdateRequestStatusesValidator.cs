namespace Rassef.Validators.RequestStatusesValidators
{
    public class UpdateRequestStatusesValidator : AbstractValidator<UpdateRequestStatusesVM>
    {
        public UpdateRequestStatusesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف حالة الطلب غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة الطلب.")
                .MaximumLength(100).WithMessage("اسم حالة الطلب يجب ألا يتجاوز 100 حرف.");
        }
    }
}
