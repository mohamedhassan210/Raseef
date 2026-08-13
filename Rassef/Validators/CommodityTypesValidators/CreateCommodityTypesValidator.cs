namespace Rassef.Validators.CommodityTypes
{
    public class CreateCommodityTypesValidator : AbstractValidator<CreateCommodityTypesVM>
    {
        public CreateCommodityTypesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع السلعة.")
                .MaximumLength(100).WithMessage("اسم نوع السلعة يجب ألا يتجاوز 100 حرف.");
        }
    }
}
