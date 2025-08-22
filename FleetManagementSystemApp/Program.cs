using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.SubModelHandlers;
using FleetManagementSystemApp.Configs;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Data.SeedDB;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.Infrastructure.ModelBinders;
using FleetManagementSystemApp.Logging;
using FleetManagementSystemApp.Middleware;
using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Vehicle;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Logging.ClearProviders();
builder.Host.UseSerilog((ctx, services, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithMachineName()
      .Enrich.WithEnvironmentName()
      .Enrich.WithThreadId()
      .Enrich.WithCorrelationId()
      .Enrich.With(new UserIdEnricher(services.GetRequiredService<IHttpContextAccessor>()));

    if (ctx.HostingEnvironment.IsDevelopment())
    {
        lc.WriteTo.Console();
    }
});

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "fms_app:";
});
builder.Services.AddLazyCache();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton<IHybridCache, HybridCache>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;

    // User settings
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.AllowedForNewUsers = true; // Activates user lockout to prevent brute force attacks targeting user passwords
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Password settings
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddSingleton<ILogEventEnricher, UserIdEnricher>();
builder.Services.AddOptions<EmailSettings>()
                .Bind(builder.Configuration.GetSection("EmailSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAggregateModelService<VehiclePageViewModel>, AggregateModelService<VehiclePageViewModel>>();
builder.Services.AddScoped<IVehicleDataAggregator, VehicleDataAggregator>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<IConfirmationService, ConfirmationEmailService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<DateRangeParser>();
builder.Services.AddScoped<ApplicationUserDtoExtentions>();
builder.Services.AddScoped<AddressDtoExtentions>();
builder.Services.AddScoped<CompanyDtoExtentions>();
builder.Services.AddScoped<VehicleDtoExtentions>();
builder.Services.AddScoped<PassportDtoExtentions>();
builder.Services.AddScoped<InsuranceDtoExtentions>();
builder.Services.AddScoped<RegistrationCertificateDtoExtentions>();
builder.Services.AddScoped<VehicleIdentificationDataDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Vehicle, VehicleDto>, VehicleDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Passport, PassportDto>, PassportDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Insurance, InsuranceDto>, InsuranceDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<RegistrationCertificate, RegistrationCertificateDto>, RegistrationCertificateDtoExtentions>();
builder.Services.AddScoped<ISeederDatabase, SeederDatabase>();
builder.Services.AddScoped<ISubModelHandlerFactory, SubModelHandlerFactory>();
builder.Services.AddScoped<ISubModelHandler, PassportSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, InsuranceSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, RegistrationCertificateSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, VehicleIdentificationDataSubModelHandler>();
// Configuring DataProtection key storage; change this in production!
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys\"))
    .SetApplicationName("FleetManagementSystem");
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "fmsAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax; // Protection from CSRF
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.ModelBinderProviders.Insert(0, new DateRangeModelBinderProvider());
    })
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen(options =>
{
    //Configuring Swagger to read XML comments
    string xmlPath = Path.Combine(Environment.CurrentDirectory, "documentation.xml");
    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
StartupChecks.ValidateRequiredSettings(builder.Configuration); //Validation environment variables, etc.

/***Pipeline***/

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStaticFiles();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        diagnosticContext.Set("UserAgent", userAgent);

        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        diagnosticContext.Set("RemoteIpAddress", remoteIp);
    };
    options.GetLevel = (httpContext, elapsed, exception) =>
    {
        if (exception != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
});
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only in this sequence: UseRouting() -> UseAuthentication() -> UseAuthorization()
app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Vehicles}/{action=List}/{id?}");

app.MapControllers();
app.MapRazorPages();

// Initialization of data
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ISeederDatabase>();
    await seeder.SeedAsync();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/vehicles");
    return Task.CompletedTask;
});

app.Run();