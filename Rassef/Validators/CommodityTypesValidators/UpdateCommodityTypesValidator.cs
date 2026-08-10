namespace Rassef.Validators.CommodityTypes
{
    public class UpdateCommodityTypesValidator : AbstractValidator<UpdateCommodityTypesVM>
    {
        public UpdateCommodityTypesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع السلعة غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع السلعة.")
                .MaximumLength(100).WithMessage("اسم نوع السلعة يجب ألا يتجاوز 100 حرف.");
        }
    }
}
