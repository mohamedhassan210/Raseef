using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Rassef.Dependencyinjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependcyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Add SqlServer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddLogging();

            // 2. Add Controllers & Filters
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<Rassef.Filters.FluentValidationActionFilter>();
            });

            services.AddExceptionHandler<GlobalExceptionHandling>();
            services.AddProblemDetails();
            services.AddValidatorsFromAssemblyContaining<Program>();

            // 3. Add Repositories & Services
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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
            services.AddScoped<IPermitTypeRepository, PermitTypeRepository>();
            services.AddScoped<IRequestStatusRepository, RequestStatusRepository>();
            services.AddScoped<ExcelExportService>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IJwtService, JwtService>();

            // 4. JWT Settings Configuration
            var jwtSettingsSection = configuration.GetSection("Jwt");
            services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key ?? "YourDefaultSecretKeyHere")),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                // 💡 قراءة التوكين تلقائياً من الـ Cookie المسماة "AccessToken"
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // 6. Serilog Configuration
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
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<LoggingBehavior>();

            // 🛑 الترتيب مهم جداً هنا:
            app.UseAuthentication(); // 1. التعرف على هُوية المستخدم من الـ Cookie/JWT أولاً
            app.UseAuthorization();  // 2. ثم فحص صلاحياته

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Intro}/{id?}");

            return app;
        }
    }
}