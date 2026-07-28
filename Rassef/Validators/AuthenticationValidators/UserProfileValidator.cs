namespace Rassef.Validators.AuthenticationValidators
{
    public class UserProfileValidator : AbstractValidator<UserProfileViewModel>
    {
        public UserProfileValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");


            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

            RuleFor(x => x.Email)
             .NotNull().WithMessage("Email is required.")
              .DependentRules(() =>
              {
                  RuleFor(x => x.Email.Value)
               .NotEmpty().WithMessage("Email is required.")
               .EmailAddress().WithMessage("Please enter a valid email address.");
              });


            RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("National ID is required.")
                .Matches(@"^\d{14}$").WithMessage("National ID must contain exactly 14 digits.");
        }
    }
}
