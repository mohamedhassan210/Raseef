
namespace Rassef.Validators.Department
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentVM>
    {
        private readonly IRepository<Models.Entities.Department> _departmentRepository;

        public UpdateDepartmentValidator(IRepository<Models.Entities.Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("معرف القسم مطلوب.");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("يرجى اختيار المخزن.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم القسم مطلوب.")
                .Length(3, 100).WithMessage("يجب أن يكون اسم القسم بين 3 و100 حرف.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueNameInWarehouseExceptCurrent(model))
                .WithMessage("اسم القسم مستخدم بالفعل في هذا المخزن.")
                .OverridePropertyName(nameof(UpdateDepartmentVM.Name));
        }

        private async Task<bool> BeUniqueNameInWarehouseExceptCurrent(UpdateDepartmentVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.WarehouseId == Guid.Empty)
                return true;

            bool exists = await _departmentRepository.ExistsAsync(
                d => d.Name.Trim().ToLower() == model.Name.Trim().ToLower()
                  && d.WarehouseId == model.WarehouseId
                  && d.Id != model.Id
            );

            return !exists;
        }
    }
}