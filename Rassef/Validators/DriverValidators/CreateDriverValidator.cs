using FluentValidation;
using Rassef.Common.Interfaces;
using Rassef.ViewModels.Driver;

namespace Rassef.Validators.Driver
{
    public class CreateDriverVMValidator : AbstractValidator<CreateDriverVM>
    {
        private readonly IRepository<Models.Entities.Driver> _driverRepository;

        public CreateDriverVMValidator(IRepository<Models.Entities.Driver> driverRepository)
        {
            _driverRepository = driverRepository;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("اسم السائق مطلوب.")
                .Length(3, 100).WithMessage("يجب أن يكون اسم السائق بين 3 و100 حرف.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.")
                .Matches(@"^\d{14}$").WithMessage("يجب أن يتكون الرقم القومي من 14 رقمًا.")
                .MustAsync((nationalId, _) => BeUniqueNationalId(nationalId))
                .WithMessage("الرقم القومي مسجل بالفعل لسائق آخر.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^01[0125][0-9]{8}$").WithMessage("رقم الهاتف غير صحيح (يجب أن يبدأ بـ 010 أو 011 أو 012 أو 015 ويتكون من 11 رقمًا).")
                .MustAsync((phone, _) => BeUniquePhone(phone))
                .WithMessage("رقم الهاتف مسجل بالفعل لسائق آخر.");
        }

        private async Task<bool> BeUniqueNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return true;

            bool exists = await _driverRepository.ExistsAsync(
                d => d.NationalId.Trim() == nationalId.Trim()
            );

            return !exists;
        }

        private async Task<bool> BeUniquePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            bool exists = await _driverRepository.ExistsAsync(
                d => d.Phone.Trim() == phone.Trim()
            );

            return !exists;
        }
    }
}