using Rassef.Common.Repository;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace Rassef.Dependencyinjection
{
    // Eexstension Method
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependcyInjection(this IServiceCollection services)
        {
            // add services 
            services.AddControllersWithViews();
            services.AddExceptionHandler<GlobalExceptionHandling>();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddScoped(typeof(IRepository<>),typeof( Repository<>));


            return services;
        }
        public static WebApplication AddMiddleWares(this WebApplication app)
        {
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Index}/{id?}");
            app.UseMiddleware<LogginBehaviors>();
            return app;
        }
    }
}
