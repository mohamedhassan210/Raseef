namespace Rassef.Dependencyinjection
{
    // Eexstension Method
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependcyInjection(this IServiceCollection services , IConfiguration configuration)
        {
            // add services 
            services.AddControllersWithViews();
            services.AddExceptionHandler<GlobalExceptionHandling>();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //add services 
            services.AddScoped<ICheckOutRepository, CheckOutRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDockAssignmentRepository, DockAssignmentRepository>();
            services.AddScoped<IDockRepository, DockRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IQueueActionRepository,QueueActionRepository >();
            services.AddScoped<IQueueTicketRepository,QueueTicketRepository >();
            services.AddScoped<ISupplierRepository,SupplierRepository >();
            services.AddScoped<ISupplierRequestRepository,SupplierRequestRepository >();
            services.AddScoped<ITransferRequestRepository,TransferRequestRepository >();
            services.AddScoped<ITruckRepository,TruckRepository >();
            services.AddScoped<IWarehouseRepository,WarehouseRepository >();

            // JWT Configuration
            services.Configure<JwtSettings>(
      configuration.GetSection("Jwt"));

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Key)),

                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["AccessToken"];
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();


            return services;
        }
        public static WebApplication AddMiddleWares(this WebApplication app)
        {
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Register}/{id?}");
            app.UseMiddleware<LogginBehaviors>();
            return app;
        }
    }
}
