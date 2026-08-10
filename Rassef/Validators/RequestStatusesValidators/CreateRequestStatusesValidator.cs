namespace Rassef.Validators.RequestStatusesValidators
{
    public class CreateRequestStatusesValidator : AbstractValidator<CreateRequestStatusesVM>
    {
        public CreateRequestStatusesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة الطلب.")
                .MaximumLength(100).WithMessage("اسم حالة الطلب يجب ألا يتجاوز 100 حرف.");
        }
    }
}
