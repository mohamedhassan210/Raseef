namespace Rassef.Validators.Warehouse
{
    public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseVM>
    {
        private readonly IRepository<Models.Entities.Warehouse> _warehouseRepository;

        public CreateWarehouseValidator(IRepository<Models.Entities.Warehouse> warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المخزن مطلوب.")
                .Length(2, 100).WithMessage("يجب أن يكون اسم المخزن بين 2 و 100 حرف.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("الموقع مطلوب.")
                .Length(3, 200).WithMessage("يجب أن يكون الموقع بين 3 و 200 حرف.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueWarehouseName(model))
                .WithMessage("اسم المستودع موجود بالفعل.")
                .OverridePropertyName(nameof(CreateWarehouseVM.Name));
        }

        private async Task<bool> BeUniqueWarehouseName(CreateWarehouseVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return true;

            bool exists = await _warehouseRepository.ExistsAsync(
                w => w.Name.Trim().ToLower() == model.Name.Trim().ToLower()
            );

            return !exists;
        }
    }
}