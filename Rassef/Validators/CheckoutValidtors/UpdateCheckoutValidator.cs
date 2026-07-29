

namespace Rassef.Validators.CheckOut
{
    public class UpdateCheckOutVMValidator : AbstractValidator<UpdateCheckOutVM>
    {
        private readonly IRepository<Models.Entities.CheckOut> _checkOutRepository;

        public UpdateCheckOutVMValidator(IRepository<Models.Entities.CheckOut> checkOutRepository)
        {
            _checkOutRepository = checkOutRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("المعرف (ID) مطلوب.");

            RuleFor(x => x.TicketId)
                .NotEmpty().WithMessage("يرجى اختيار التذكرة.");

            RuleFor(x => x.ExitTypeId)
                .NotEmpty().WithMessage("يرجى اختيار نوع الخروج.");

            RuleFor(x => x.ExitTime)
                .NotEmpty().WithMessage("يرجى إدخال وقت الخروج.")
                .LessThanOrEqualTo(DateTimeOffset.Now).WithMessage("وقت الخروج لا يمكن أن يكون في المستقبل.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueCheckOutForTicketExceptCurrent(model))
                .WithMessage("تم تسجيل خروج لهذه التذكرة بالفعل في عملية أخرى.")
                .OverridePropertyName(nameof(UpdateCheckOutVM.TicketId));
        }

        private async Task<bool> BeUniqueCheckOutForTicketExceptCurrent(UpdateCheckOutVM model)
        {
            if (model.TicketId == default)
                return true;

            bool exists = await _checkOutRepository.ExistsAsync(
                c => c.TicketId == model.TicketId
                  && c.Id != model.Id
            );

            return !exists;
        }
    }
}