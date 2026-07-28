
namespace Rassef.Validators.Warehouse
{
    public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseVM>
    {
        private readonly IRepository<Models.Entities.Warehouse> _warehouseRepository;

        public UpdateWarehouseValidator(IRepository<Models.Entities.Warehouse> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("معرف المخزن مطلوب.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المخزن مطلوب.")
                .Length(2, 100).WithMessage("يجب أن يكون اسم المخزن بين 2 و 100 حرف.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("الموقع مطلوب.")
                .Length(3, 200).WithMessage("يجب أن يكون الموقع بين 3 و 200 حرف.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueWarehouseNameExceptCurrent(model))
                .WithMessage("اسم المستودع مستخدم بالفعل لمستودع آخر.")
                .OverridePropertyName(nameof(UpdateWarehouseVM.Name));
        }

        private async Task<bool> BeUniqueWarehouseNameExceptCurrent(UpdateWarehouseVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return true;

            bool exists = await _warehouseRepository.ExistsAsync(
                w => w.Name.Trim().ToLower() == model.Name.Trim().ToLower()
                  && w.Id != model.Id
            );

            return !exists;
        }
    }
}