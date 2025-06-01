using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Configs;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
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
builder.Services.AddOptions<EmailSettings>()
                .Bind(builder.Configuration.GetSection("EmailSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ApplicationUserDtoExtentions>();
builder.Services.AddScoped<AddressDtoExtentions>();
builder.Services.AddScoped<CompanyDtoExtenctions>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IConfirmationService, ConfirmationEmailService>();
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
    );
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
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

StartupChecks.ValidateRequiredSettings(builder.Configuration); //Validation environment variables, etc.

/***Pipeline***/

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
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
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autopark}/{action=Vehicles}/{id?}");

app.MapControllers();
app.MapRazorPages();

app.Run();