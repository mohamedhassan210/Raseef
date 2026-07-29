
namespace Rassef.Validators.Dock
{
    public class UpdateDockValidator : AbstractValidator<UpdateDockVM>
    {
        private readonly IRepository<Models.Entities.Dock> _dockRepository;

        public UpdateDockValidator(IRepository<Models.Entities.Dock> dockRepository)
        {
            _dockRepository = dockRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("المعرف (ID) مطلوب.");

            RuleFor(x => x.DockName)
                .NotEmpty().WithMessage("يرجى إدخال اسم الرصيف.")
                .MaximumLength(100).WithMessage("اسم الرصيف يجب ألا يتجاوز 100 حرف.");

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("يرجى اختيار القسم.");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("يرجى اختيار المخزن.");

            RuleFor(x => x.DockStatusId)
                .NotEmpty().WithMessage("يرجى اختيار حالة الرصيف.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueDockNameExceptCurrent(model))
                .WithMessage("اسم الرصيف مستخدم بالفعل في هذا القسم.")
                .OverridePropertyName(nameof(UpdateDockVM.DockName));
        }

        private async Task<bool> BeUniqueDockNameExceptCurrent(UpdateDockVM model)
        {
            if (string.IsNullOrWhiteSpace(model.DockName) || model.DepartmentId == default)
                return true;

            bool exists = await _dockRepository.ExistsAsync(
                d => d.DockName.Trim().ToLower() == model.DockName.Trim().ToLower()
                  && d.DepartmentId == model.DepartmentId
                  && d.Id != model.Id
            );

            return !exists;
        }
    }
}