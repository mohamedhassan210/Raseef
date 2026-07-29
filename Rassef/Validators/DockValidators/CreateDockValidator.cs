namespace Rassef.Validators.Dock
{
    public class CreateDockValidator : AbstractValidator<CreateDockVM>
    {
        private readonly IRepository<Models.Entities.Dock> _dockRepository;

        public CreateDockValidator(IRepository<Models.Entities.Dock> dockRepository)
        {
            _dockRepository = dockRepository;

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
                .MustAsync((model, _) => BeUniqueDockName(model))
                .WithMessage("اسم الرصيف موجود بالفعل في هذا القسم لهذا المخزن.")
                .OverridePropertyName(nameof(CreateDockVM.DockName));
        }

        private async Task<bool> BeUniqueDockName(CreateDockVM model)
        {
            if (string.IsNullOrWhiteSpace(model.DockName) || model.DepartmentId ==default)
                return true;

            bool exists = await _dockRepository.ExistsAsync(
                d => d.DockName.Trim().ToLower() == model.DockName.Trim().ToLower()
                  && d.DepartmentId == model.DepartmentId
            );

            return !exists;
        }
    }
}
