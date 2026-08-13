using Microsoft.AspNetCore.Mvc.Filters;

namespace Rassef.Filters
{
    public class FluentValidationActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.Any())
            {
                var serviceProvider = context.HttpContext.RequestServices;

                foreach (var arg in context.ActionArguments.Values)
                {
                    if (arg == null) continue;

                    var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                    var validator = serviceProvider.GetService(validatorType) as IValidator;

                    if (validator != null)
                    {
                        var validationContext = new ValidationContext<object>(arg);
                        var validationResult = await validator.ValidateAsync(validationContext);

                        if (!validationResult.IsValid)
                        {
                            foreach (var error in validationResult.Errors)
                            {
                                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                            }
                        }
                    }
                }
            }

            await next();
        }
    }
}
