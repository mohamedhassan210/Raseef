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
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<Rassef.Filters.FluentValidationActionFilter>();
            });
            services.AddExceptionHandler<GlobalExceptionHandling>();
            services.AddProblemDetails();
            services.AddValidatorsFromAssemblyContaining<Program>(); // add validators 
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
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtSettings>(
              configuration.GetSection("Jwt"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.File(
                    "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();


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

            app.UseMiddleware<LoggingBehavior>();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Intro}/{id?}");
            return app;
        }
    }
}
