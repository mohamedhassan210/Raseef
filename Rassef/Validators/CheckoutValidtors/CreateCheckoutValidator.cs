
namespace Rassef.Validators.CheckOut
{
    public class CreateCheckOutValidator : AbstractValidator<CreateCheckOutVM>
    {
        private readonly IRepository<Models.Entities.CheckOut> _checkOutRepository;

        public CreateCheckOutValidator(IRepository<Models.Entities.CheckOut> checkOutRepository)
        {
            _checkOutRepository = checkOutRepository;

            RuleFor(x => x.TicketId)
                .NotEmpty().WithMessage("يرجى اختيار التذكرة.");


            RuleFor(x => x.ExitTypeId)
                .NotEmpty().WithMessage("يرجى اختيار نوع الخروج.");

            RuleFor(x => x.ExitTime)
                .NotEmpty().WithMessage("يرجى إدخال وقت الخروج.")
                .LessThanOrEqualTo(DateTimeOffset.Now).WithMessage("وقت الخروج لا يمكن أن يكون في المستقبل.");

            RuleFor(x => x)
                .MustAsync((model, _) => BeUniqueCheckOutForTicket(model))
                .WithMessage("تم تسجيل خروج لهذه التذكرة بالفعل مسبقاً.")
                .OverridePropertyName(nameof(CreateCheckOutVM.TicketId));
        }

        private async Task<bool> BeUniqueCheckOutForTicket(CreateCheckOutVM model)
        {
            if (model.TicketId == default)
                return true;

            bool exists = await _checkOutRepository.ExistsAsync(
                c => c.TicketId == model.TicketId
            );

            return !exists;
        }
    }
}