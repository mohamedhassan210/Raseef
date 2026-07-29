
namespace Rassef.Validators.SupplierRequestValidators
{
    public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequestVM>
    {
        public UpdateSupplierRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("المعرف (ID) غير صالح.");

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("برجاء اختيار المورد بشكل صحيح.");

            RuleFor(x => x.TruckId)
                .GreaterThan(0).WithMessage("برجاء اختيار الشاحنة بشكل صحيح.");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage("برجاء اختيار السائق بشكل صحيح.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("برجاء اختيار القسم بشكل صحيح.");

            RuleFor(x => x.PermitTypeId)
                .GreaterThan(0).WithMessage("برجاء اختيار نوع التصريح بشكل صحيح.");

            RuleFor(x => x.CommodityTypeId)
                .GreaterThan(0).WithMessage("برجاء اختيار نوع السلعة بشكل صحيح.");

            RuleFor(x => x.RequestStatusId)
                .GreaterThan(0).WithMessage("برجاء اختيار حالة الطلب بشكل صحيح.");

            RuleFor(x => x.DriverPhone)
                .NotEmpty().WithMessage("برجاء إدخال رقم هاتف السائق.")
                .Matches(@"^(010|011|012|015)\d{8}$")
                .WithMessage("رقم هاتف السائق غير صالح (يجب أن يكون رقم مصري مكون من 11 رقم).");

            RuleFor(x => x.PermitNumber)
                .NotEmpty().WithMessage("برجاء إدخال رقم التصريح.")
                .MaximumLength(50).WithMessage("رقم التصريح يجب ألا يتجاوز 50 حرفًا.");
        }
    }
}