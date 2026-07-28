

namespace Rassef.Validators.Truck
{
    public class CreateTruckValidator : AbstractValidator<CreateTruckVM>
    {
        private readonly IRepository<Models.Entities.Truck> _truckRepository;

        public CreateTruckValidator(IRepository<Models.Entities.Truck> truckRepository)
        {
            _truckRepository = truckRepository;

            RuleFor(x => x.PlateNumber)
                .NotEmpty().WithMessage("رقم اللوحة مطلوب.")
                .Length(1, 10).WithMessage("يجب أن يكون رقم اللوحة بين 1 و10 أحرف.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("رقم اللوحة يجب أن يحتوي على حروف إنجليزية وأرقام فقط.");

            RuleFor(x => x.PlateLetter)
                .NotEmpty().WithMessage("حروف اللوحة مطلوبة.")
                .Length(1, 5).WithMessage("يجب أن تتكون حروف اللوحة من 1 إلى 5 أحرف.")
                .Matches(@"^[A-Za-z]+$").WithMessage("حروف اللوحة يجب أن تحتوي على حروف إنجليزية فقط.");

            RuleFor(x => x.StorageCapacity)
                .InclusiveBetween(0.1, 1000).WithMessage("يجب أن تكون السعة التخزينية بين 0.1 و1000 طن.");

            RuleFor(x => x.TruckTypeId)
                .NotEmpty().WithMessage("يرجى اختيار نوع الشاحنة.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniquePlate(model))
                .WithMessage("هذه الشاحنة مسجلة بالفعل بنفس رقم وحروف اللوحة.")
                .OverridePropertyName(nameof(CreateTruckVM.PlateNumber));
        }

        private async Task<bool> BeUniquePlate(CreateTruckVM model)
        {
            if (string.IsNullOrWhiteSpace(model.PlateNumber) || string.IsNullOrWhiteSpace(model.PlateLetter))
                return true;

            bool exists = await _truckRepository.ExistsAsync(
                t => t.PlateNumber.Trim().ToLower() == model.PlateNumber.Trim().ToLower()
                  && t.PlateLetter.Trim().ToLower() == model.PlateLetter.Trim().ToLower()
            );

            return !exists;
        }
    }
}