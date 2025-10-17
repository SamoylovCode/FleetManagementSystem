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

//builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
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
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Password settings
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

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
builder.Services.AddScoped<CertificateTechInspectionExtentions>();
builder.Services.AddScoped<VehicleIdentificationDataDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Vehicle, VehicleDto>, VehicleDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Passport, PassportDto>, PassportDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<Insurance, InsuranceDto>, InsuranceDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<RegistrationCertificate, RegistrationCertificateDto>, RegistrationCertificateDtoExtentions>();
builder.Services.AddScoped<IBaseMapper<CertificateTechInspection, CertificateTechInspectionDto>, CertificateTechInspectionExtentions>();
builder.Services.AddScoped<ISeederDatabase, SeederDatabase>();
builder.Services.AddScoped<ISubModelHandlerFactory, SubModelHandlerFactory>();
builder.Services.AddScoped<ISubModelHandler, PassportSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, InsuranceSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, RegistrationCertificateSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, VehicleIdentificationDataSubModelHandler>();
builder.Services.AddScoped<ISubModelHandler, CertificateTechInspectionSubModelHandler>();
// Configuring DataProtection key storage; change this in production!
builder.Services.AddDataProtection()
    .SetApplicationName("FleetManagementSystemApp")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys/")) // For Linux
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    //.UseEphemeralDataProtectionProvider(); // <-- ИСПОЛЬЗОВАТЬ ТОЛЬКО ДЛЯ РАЗРАБОТКИ!
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "fms.af";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.None; // в Dev без https
    // o.HeaderName = "X-CSRF-TOKEN"; // опция, если отправлять токен в header
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "fms.auth";
    options.Cookie.HttpOnly = true;
    //options.SecurePolicy = CookieSecurePolicy.None;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax; // Protection from CSRF
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});
//builder.Services.AddControllers() // Только API
//    .AddControllersAsServices();
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
builder.Services.AddHealthChecks();
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
    // builder.Services.ConfigureApplicationCookie(o =>
    // {
    //     o.Cookie.SecurePolicy = CookieSecurePolicy.None;
    // });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Only in this sequence: UseRouting() -> UseAuthentication() -> UseAuthorization()
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Vehicles}/{action=List}/{id?}");

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/health");

// Initialization of data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<ApplicationDbContext>();
    var log = services.GetService<ILogger<Program>>();

    try
    {
        if(dbContext != null)
        {
            log?.LogDebug("Applying migrations to DB.");
            await dbContext.Database.MigrateAsync();
        }

        //var seeder = services.GetRequiredService<ISeederDatabase>();
        //await seeder.SeedAsync();
    }
    catch (Exception e)
    {
        log?.LogError(e, "An error occurred while migrating/seeding the database.");
    }
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/vehicles");
    return Task.CompletedTask;
});

app.Run();