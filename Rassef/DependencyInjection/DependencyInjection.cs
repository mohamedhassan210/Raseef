namespace Rassef.Dependencyinjection
{
    // Eexstension Method
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependcyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // add sqlserver
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddLogging();
            // add services 
            services.AddControllersWithViews();
            services.AddExceptionHandler<GlobalExceptionHandling>();
            services.AddProblemDetails();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //add services 
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICheckOutRepository, CheckOutRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDockAssignmentRepository, DockAssignmentRepository>();
            services.AddScoped<IDockRepository, DockRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IQueueActionRepository, QueueActionRepository>();
            services.AddScoped<IQueueTicketRepository, QueueTicketRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISupplierRequestRepository, SupplierRequestRepository>();
            services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
            services.AddScoped<ITruckRepository, TruckRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtSettings>(
              configuration.GetSection("Jwt"));



            return services;
        }
        public static WebApplication AddMiddleWares(this WebApplication app)
        {
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.Redirect("/Home/Error");
                });
            }); app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Intro}/{id?}");
            app.UseMiddleware<LogginBehaviors>();
            return app;
        }
    }
}
