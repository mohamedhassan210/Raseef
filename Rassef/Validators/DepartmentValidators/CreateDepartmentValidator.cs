
namespace Rassef.Validators.Department
{
    public class CreateDepartmentVMValidator : AbstractValidator<CreateDepartmentVM>
    {
        private readonly IRepository<Models.Entities.Department> _departmentRepository;

        public CreateDepartmentVMValidator(IRepository<Models.Entities.Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("يرجى اختيار المخزن.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم القسم مطلوب.")
                .Length(3, 100).WithMessage("يجب أن يكون اسم القسم بين 3 و100 حرف.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueNameInWarehouse(model))
                .WithMessage("اسم القسم موجود بالفعل في هذا المخزن.")
                .OverridePropertyName(nameof(CreateDepartmentVM.Name));
        }

        private async Task<bool> BeUniqueNameInWarehouse(CreateDepartmentVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.WarehouseId == Guid.Empty)
                return true;

            bool exists = await _departmentRepository.ExistsAsync(
                d => d.Name.Trim().ToLower() == model.Name.Trim().ToLower()
                  && d.WarehouseId == model.WarehouseId
            );

            return !exists;
        }
    }
}