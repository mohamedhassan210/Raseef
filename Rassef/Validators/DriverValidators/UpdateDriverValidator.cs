

namespace Rassef.Validators.Driver
{
    public class UpdateDriverValidator : AbstractValidator<UpdateDriverVM>
    {
        private readonly IRepository<Models.Entities.Driver> _driverRepository;

        public UpdateDriverValidator(IRepository<Models.Entities.Driver> driverRepository)
        {
            _driverRepository = driverRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("المعرف (ID) مطلوب.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("اسم السائق مطلوب.")
                .Length(3, 100).WithMessage("يجب أن يكون اسم السائق بين 3 و100 حرف.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.")
                .Matches(@"^\d{14}$").WithMessage("يجب أن يتكون الرقم القومي من 14 رقمًا.")
                .MustAsync((model, nationalId, _) => BeUniqueNationalIdExceptCurrent(model, nationalId))
                .WithMessage("الرقم القومي مسجل بالفعل لسائق آخر.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^01[0125][0-9]{8}$").WithMessage("رقم الهاتف غير صحيح.")
                .MustAsync((model, phone, _) => BeUniquePhoneExceptCurrent(model, phone))
                .WithMessage("رقم الهاتف مسجل بالفعل لسائق آخر.");
        }

        private async Task<bool> BeUniqueNationalIdExceptCurrent(UpdateDriverVM model, string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return true;

            bool exists = await _driverRepository.ExistsAsync(
                d => d.NationalId.Trim() == nationalId.Trim()
                  && d.Id != model.Id
            );

            return !exists;
        }

        private async Task<bool> BeUniquePhoneExceptCurrent(UpdateDriverVM model, string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            bool exists = await _driverRepository.ExistsAsync(
                d => d.Phone.Trim() == phone.Trim()
                  && d.Id != model.Id
            );

            return !exists;
        }
    }
}