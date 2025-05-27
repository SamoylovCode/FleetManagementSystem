using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Account;
using FleetManagementSystemApp.ViewModels.Admin;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystemApp.Business.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationUserDtoExtentions _userMapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly LinkGenerator _linkGenerator;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserCreationValidator _validator;
    private readonly UserLoginValidator _loginValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(ApplicationUserDtoExtentions userDto,
                       UserManager<ApplicationUser> userManager,
                       RoleManager<IdentityRole> roleManager,
                       UserCreationValidator validator,
                       IServiceProvider serviceProvider,
                       ApplicationDbContext dbContext,
                       IHttpContextAccessor contextAccessor,
                       UserLoginValidator loginValidator,
                       SignInManager<ApplicationUser> signInManager,
                       ICurrentUserService currentUserService,
                       IEmailSender emailSender,
                       LinkGenerator linkGenerator)
    {
        _userMapper = userDto;
        _userManager = userManager;
        _roleManager = roleManager;
        _validator = validator;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _loginValidator = loginValidator;
        _signInManager = signInManager;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _linkGenerator = linkGenerator;
    }

    private async Task<List<Claim>> CreateUserClaims(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.GivenName, $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim()),
            new Claim("CompanyId", user.CompanyId.ToString())
        };

        if (!string.IsNullOrEmpty(role))
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userRoles = userRoles.Append(role).Distinct().ToList();

            foreach(var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }
        }

        return claims;
    }

    public async Task UpdateUserClaimsAsync(ApplicationUser user)
    {
        // Update claims in database
        var currentClaims = await _userManager.GetClaimsAsync(user);
        await _userManager.RemoveClaimsAsync(user, currentClaims);

        var newClaims = await CreateUserClaims(user, null);
        await _userManager.AddClaimsAsync(user, newClaims);

        // Update current session
        await _signInManager.RefreshSignInAsync(user);
    }

    public async ValueTask<Result<List<ApplicationUserDto>>> GetAllUsersListAsync()
    {
        var companyId = _currentUserService.CompanyId;
        if (string.IsNullOrEmpty(companyId))
        {
            return Result<List<ApplicationUserDto>>.Failure("Не содержится сведений об организации пользователя, совершающего запрос.");
        }
        var users = await _userManager.Users
            .Where(u => u.CompanyId == Guid.Parse(companyId))
            .AsNoTracking()
            .ToListAsync();

        return users is not null
            ? _userMapper.ToDto(users)
            : Result<List<ApplicationUserDto>>.Failure("В базе данных не содержится ни одного пользователя.");
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result<ApplicationUserDto>.Failure("ID пользователя не указан.");
        }

        var user = await _userManager.FindByIdAsync(id);

        return user is not null
            ? _userMapper.ToDto(user)
            : Result<ApplicationUserDto>.Failure("Пользователь не найден.");
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<ApplicationUserDto>.Failure("Email не указан.");
        }

        var user = await _userManager.FindByEmailAsync(email);

        return user is not null
            ? _userMapper.ToDto(user)
            : Result<ApplicationUserDto>.Failure("Указанный электронный адрес не привязан ни к одному пользователю.");
    }

    public async ValueTask<IdentityResult> LoginUserAsync(LoginViewModel model)
    {
        var validationResult = _loginValidator.Validate(model);
        if (!validationResult.IsValid)
        {
            return IdentityResult.Failed(
                validationResult.Errors
                .Select(e => new IdentityError { Description = e.ErrorMessage })
                .ToArray()
                );
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Не удалось найти пользователя."
            });
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!checkPassword)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Неверный пароль."
            });
        }
        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> AddUserAsync(AddUserViewModel model, string scheme)
    {
        using(var transaction = _dbContext.Database.BeginTransaction())
        {
            var currentUserCompanyId = _currentUserService.CompanyId;
            if (string.IsNullOrEmpty(currentUserCompanyId))
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(nameof(currentUserCompanyId));
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email, // The username is the same as the email value.
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                CompanyId = Guid.Parse(currentUserCompanyId), // Applying admin's CompanyId
                CreatedAt = DateTime.UtcNow // Registration date, set current time
            };

            // TODO: EmailConfirmed service

            // Validation of params using FluentValidation
            var validator = _serviceProvider.GetRequiredService<IValidator<(ApplicationUser addUser, string? password, string? role)>>();
            var validationResult = await validator.ValidateAsync((user, null, model.Role));
            if (!validationResult.IsValid)
            {
                return IdentityResult.Failed(
                    validationResult.Errors
                    .Select(e => new IdentityError { Description = e.ErrorMessage })
                    .ToArray()
                );
            }

            var creatingResult = await _userManager.CreateAsync(user);
            if (!creatingResult.Succeeded)
            {
                return IdentityResult.Failed(creatingResult.Errors.ToArray());
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addRoleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось применить роль к пользователю."
                });
            }

            var claims = await CreateUserClaims(user, model.Role);
            var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
            if (!addClaimsResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось применить утверждения к пользователю."
                });
            }

            var confirmEmailResult = await SendConfirmationEmailAsync(user, scheme);

            if (!confirmEmailResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось отправить письмо с подтверждением регистрации."
                });
            }
            transaction.Commit();
        }

        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> CreateUserAsync(RegisterViewModel model, string scheme)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            var company = new Company
            {
                CompanyId = Guid.NewGuid(),
                CompanyName = model.CompanyName,
                PhoneNum = model.PhoneNum,
                Inn = model.Inn,
                Kpp = model?.Kpp,
                Ogrn = model?.Ogrn,
                Okpo = model?.Okpo,
                IsMain = true
            };

            var address = new Address
            {
                CompanyId = company.CompanyId,
                Region = model.Region,
                City = model.City,
                Street = model.Street,
                House = model.House,
                Building = model?.Building,
                Apartment = model?.Apartment
            };

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email, //The username is the same as the email value.
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                CompanyId = company.CompanyId,
                CreatedAt = DateTime.UtcNow //Registration date, set current time
            };

            // Validation of params using FluentValidation
            var validator = _serviceProvider.GetRequiredService<IValidator<(ApplicationUser addUser, string? password, string? role)>>();
            var validationResult = await _validator.ValidateAsync((user, model.Password, ApplicationRole.Admin));
            if (!validationResult.IsValid)
            {
                return IdentityResult.Failed(
                    validationResult.Errors
                    .Select(e => new IdentityError { Description = e.ErrorMessage })
                    .ToArray()
                );
            }
            
            _dbContext.Companies.Add(company);
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось создать пользователя."
                });
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, ApplicationRole.Admin);
            if (!addRoleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось применить роль к пользователю."
                });
            }

            var claims = await CreateUserClaims(user, null);
            var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
            if (!addClaimsResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось применить утверждения к пользователю."
                });
            }

            var confirmEmailResult = await SendConfirmationEmailAsync(user, scheme);

            if (!confirmEmailResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Не удалось отправить письмо с подтверждением регистрации."
                });
            }

            await transaction.CommitAsync();
        }

        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> SetPasswordAsync(SetPassword model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = $"Пользователь с ID '{model.UserId}' не найден."
            });
        }

        var result = await _userManager.AddPasswordAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Не удалось сохранить пароль сущствующей учетной записи пользователя."
            });
        }

        return IdentityResult.Success;
    }

    // TODO: вынести в отдельный сервис!
    public async ValueTask<IdentityResult> SendConfirmationEmailAsync(ApplicationUser user, string scheme)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if(_contextAccessor.HttpContext is null)
        {
            throw new Exception("HttpContext не содержит информации о текущем запросе.");
        }
        var callbackUrl = _linkGenerator.GetUriByAction(
            httpContext: _contextAccessor.HttpContext,
            action: "Confirm",
            controller: "Account",
            values: new { userId = user.Id, token },
            scheme: scheme);

        var subject = "Подтверждение регистрации";
        var body = $"<p>Для подтверждения регистрации перейдите по <a href=\"{callbackUrl}\">ссылке</a>.</p>";

        try
        {
            await _emailSender.SendEmailAsync(user.Email!, subject, body);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Письмо не отправлено.", e);
        }
    }

    //public async ValueTask<IdentityResult> UpdateUserDataAsync(UpdateUserDataViewModel model)
    //{
    //    await UpdateUserClaimsAsync(user);
    //    //Do something
    //}

    //public ValueTask<IdentityResult> ResetUserPasswordAsync(ResetPasswordViewModel model)
    //{

    //}
}