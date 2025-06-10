using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Account;
using FleetManagementSystemApp.ViewModels.Admin;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationUserDtoExtentions _userMapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfirmationService _confirmationEmailService;
    private readonly ILogger _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserCreationValidator _validator;
    private readonly UserLoginValidator _loginValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(ApplicationUserDtoExtentions userDto,
        UserManager<ApplicationUser> userManager,
        UserCreationValidator validator,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        UserLoginValidator loginValidator,
        SignInManager<ApplicationUser> signInManager,
        ICurrentUserService currentUserService,
        IConfirmationService confirmationEmailService)
    {
        _userMapper = userDto;
        _userManager = userManager;
        _validator = validator;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _loginValidator = loginValidator;
        _signInManager = signInManager;
        _currentUserService = currentUserService;
        _confirmationEmailService = confirmationEmailService;
        _logger = Log.ForContext<UserService>();
    }

    private async Task<List<Claim>> CreateUserClaims(ApplicationUser user, string role)
    {
        _logger.Information("Создание claims для учетной записи пользователя.");
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.GivenName, $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim()),
            new Claim("CompanyId", user.CompanyId.ToString())
        };

        if (string.IsNullOrEmpty(role))
        {
            _logger.Warning("Роль не указана при создании claims.");
        }

        _logger.Information("Получение списка ролей пользователя {TargetUserId}", user.Id);
        var userRoles = await _userManager.GetRolesAsync(user);
        userRoles = userRoles.Append(role).Distinct().ToList();

        foreach(var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            _logger.Information("Добавлена роль {Role} к созданным claims.", userRole);
        }

        _logger.Debug("Claims успешно созданы.");
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
        _logger.Information("Получение списка пользователей компании {CompanyId}.", companyId);
        if (string.IsNullOrEmpty(companyId))
        {
            _logger.Warning("Контекст не содержит данных об организации.");
            return Result<List<ApplicationUserDto>>.Failure(UserServiceErrors.CompanyNotFound(null));
        }
        var users = await _userManager.Users
            .Where(u => u.CompanyId == Guid.Parse(companyId))
            .AsNoTracking()
            .ToListAsync();

        if(users.Count > 0)
        {
            _logger.Information("Возвращен список из {UsersCount} пользователей организации {CompanyId}.", users.Count, companyId);
            return _userMapper.ToDto(users);
        }
        else
        {
            _logger.Warning("Cписок пользователей организации {CompanyId} пуст.", companyId);
            return Result<List<ApplicationUserDto>>.Failure(UserServiceErrors.CompanyHasNoEmployees(companyId));
        }
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByIdAsync(string userId)
    {
        _logger.Information("Получение пользователя {TargetUserId}.", userId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.Warning("ID пользователя не указан.");
            return Result<ApplicationUserDto>.Failure(UserServiceErrors.UserIdIsNullOrEmpty());
        }

        var user = await _userManager.FindByIdAsync(userId);

        if(user is not null)
        {
            _logger.Information("Пользователь {TargetUserId} найден, возвращен объект ApplicationUserDto.", userId);
            return _userMapper.ToDto(user);
        }
        else
        {
            _logger.Warning("Пользователь {TargetUserId} не найден.", userId);
            return Result<ApplicationUserDto>.Failure(UserServiceErrors.UserNotFound(userId));
        }
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByEmailAsync(string email)
    {
        _logger.Information("Получение пользователя по Email.");
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.Warning("Email пользователя не указан.");
            return Result<ApplicationUserDto>.Failure(UserServiceErrors.EmailIsNullOrEmpty());
        }

        var user = await _userManager.FindByEmailAsync(email);

        if(user is not null)
        {
            _logger.Information("Пользователь найден, возвращен объект ApplicationUserDto.");
            return _userMapper.ToDto(user);
        }
        else
        {
            _logger.Warning("Пользователь не найден.");
            return Result<ApplicationUserDto>.Failure(UserServiceErrors.EmailNotFound());
        }
    }

    public async ValueTask<IdentityResult> LoginUserAsync(LoginViewModel model)
    {
        _logger.Information("Вход пользователя.");
        var validationResult = _loginValidator.Validate(model);
        if (!validationResult.IsValid)
        {
            _logger.Warning("Модель входа не прошла валидацию. Количество ошибок: {ErrorCount}. Поля с ошибками: {ErrorFields}",
                validationResult.Errors.Count,
                validationResult.Errors.Select(e => e.PropertyName).Distinct());
            return IdentityResult.Failed(
                validationResult.Errors.Select(e => new IdentityError {
                    Description = e.ErrorMessage
                }).ToArray());
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            _logger.Warning("Пользователь по указанному Email не найден.");
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.EmailNotFound().Description.ToString()
            });
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!checkPassword)
        {
            _logger.Warning("Неудачная попытка входа (неверный пароль).");
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.PasswordDoesNotMatch(user.Id).Description.ToString()
            });
        }
        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

        _logger.Information("Пользователь {TargetUserId} совершил вход в систему.", user.Id);
        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> AddUserAsync(AddUserViewModel model, string scheme)
    {
        var currentUserCompanyId = _currentUserService.CompanyId;
        _logger.Information("Добавление сотрудника в организацию {CompanyId}", currentUserCompanyId);
        if (string.IsNullOrEmpty(currentUserCompanyId))
        {
            _logger.Warning("Контекст не содержит данных об организации.");
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.CompanyNotFound(null).Description.ToString()
            });
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

        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            _logger.Information("Регистрация пользователя.");
            var exectuteUserCreationResult = await ExecuteUserCreationAsync(user, null, model.Role);
            if (!exectuteUserCreationResult.Succeeded)
            {
                _logger.Error("Регистрация пользователя не удалась.", exectuteUserCreationResult.Errors.First().Description);
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    //Code = UserServiceErrors.UserCreationFailed().Code.ToString(),
                    Description = UserServiceErrors.UserCreationFailed().Description.ToString()
                });
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        _logger.Information("Отправка письма для подтверждения регистрации пользователя {TargetUserId}.", user.Id);
        var confirmEmailResult = await _confirmationEmailService.SendConfirmationAsync(user, scheme);

        if (!confirmEmailResult.Succeeded)
        {
            _logger.Error("Отправка письма не удалась.");
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.SendEmailFailed(user.Id).Description.ToString()
            });
        }

        _logger.Information("Пользователь в организацию {CompanyId} успешно добавлен.", currentUserCompanyId);
        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> CreateUserAsync(RegisterViewModel model, string scheme)
    {
        _logger.Information("Регистрации пользователя");
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

        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            _dbContext.Companies.Add(company);
            _dbContext.Addresses.Add(address);

            _logger.Information("Регистрация пользователя.");
            var executeUserCreationResult = await ExecuteUserCreationAsync(user, model.Password, ApplicationRole.Admin);
            if (!executeUserCreationResult.Succeeded)
            {
                _logger.Error("Регистрация пользователя не удалась.");
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = UserServiceErrors.UserCreationFailed().Description.ToString()
                });
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        _logger.Information("Отправка письма для подтверждения регистрации пользователя {TargetUserId}.", user.Id);
        var confirmEmailResult = await _confirmationEmailService.SendConfirmationAsync(user, scheme);

        if (!confirmEmailResult.Succeeded)
        {
            _logger.Warning("Отправка письма не удалась.");
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.SendEmailFailed(user.Id).Description.ToString()
            });
        }

        _logger.Information("Регистрация пользователя {TargetUserId} прошла успешно.", user.Id);
        return IdentityResult.Success;
    }

    private async Task<IdentityResult> ExecuteUserCreationAsync(ApplicationUser user, string? password, string role)
    {
        _logger.Information("Выполнение операции регистрации пользователя.");
        var validator = _serviceProvider.GetRequiredService<IValidator<(ApplicationUser user, string? password, string role)>>();
        var validationResult = await _validator.ValidateAsync((user, password, role));
        if (!validationResult.IsValid)
        {
            _logger.Warning("Модель входа не прошла валидацию. Количество ошибок: {ErrorCount}. Поля с ошибками: {ErrorFields}",
                validationResult.Errors.Count,
                validationResult.Errors.Select(e => e.PropertyName).Distinct());
            return IdentityResult.Failed(
                validationResult.Errors.Select(e => new IdentityError
                {
                    Description = e.ErrorMessage
                }).ToArray());
        }
        var createResult = string.IsNullOrEmpty(password)
            ? await _userManager.CreateAsync(user)
            : await _userManager.CreateAsync(user, password);
        _logger.Information("Попытка создания заданного пользователя.");

        if (!createResult.Succeeded)
        {
            _logger.Error("Выполнение метода CreateAsync() завершилось ошибками: {Code}, {Description}",
                createResult.Errors.Select(e => e.Code),
                createResult.Errors.Select(e => e.Description));
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.UserCreationFailed().Description.ToString()
            });
        }
        _logger.Information("Добавление роли пользователю {TargetUserId}", user.Id);
        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            _logger.Error("Добавление роли к учетной записи пользователя {TargetUserId} не удалось.", user.Id);
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.AddToRoleFailed(user.Id).Description.ToString()
            });
        }

        var claims = await CreateUserClaims(user, role);
        _logger.Information("Добавление созданных claims к учетной записи пользователя {TargetUserId}", user.Id);
        var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
        if (!addClaimsResult.Succeeded)
        {
            _logger.Warning("Добавление claims к учетной записи пользователя {TargetUserId} не удалось", user.Id);
            return IdentityResult.Failed(new IdentityError
            {
                Description = UserServiceErrors.AddClaimsFailed(user.Id).Description.ToString()
            });
        }

        _logger.Information("Операция регистрации пользователя успешно завершена.");
        return IdentityResult.Success;
    }

    public async ValueTask<IdentityResult> SetPasswordAsync(SetPassword model)
    {
        _logger.Information("Установка пароля к учетной записи пользователя");
        await using(var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                if(model.UserId is null)
                {
                    _logger.Warning("Модель не содержит ID пользователя");
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = UserServiceErrors.UserIdIsNullOrEmpty().Description.ToString()
                    });
                }

                _logger.Information("Получение объекта пользователя {UserId}", model.UserId);
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user is null)
                {
                    _logger.Warning("Пользователь {TargetUserId} не найден.", model.UserId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = UserServiceErrors.UserNotFound(model.UserId).Description.ToString()
                    });
                }

                _logger.Information("Подтверждение Email");
                var emailConfirmed = await _userManager.ConfirmEmailAsync(user, model.Token);
                if (!emailConfirmed.Succeeded)
                {
                    _logger.Warning("Подтверждение Email не удалось");
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = UserServiceErrors.EmailConfirmedFailed(user.Id).ToString()
                    });
                }

                _logger.Information("Добавление пароля к учетной записи пользователя {TargetUserId}.", user.Id);
                var resultAddPassword = await _userManager.AddPasswordAsync(user, model.Password);
                if (!resultAddPassword.Succeeded)
                {
                    _logger.Warning("Добавление пароля к учетной записи пользователя {TargetUserId} не удалось.", user.Id);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = UserServiceErrors.AddPasswordFailed(user.Id).Description.ToString()
                    });
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.Information("Сохранение пароля пользоваетеля {TargetUserId} в БД прошло успешно.", user.Id);
            }
            catch (Exception e)
            {
                _logger.Error("Ошибка при сохранении пароля пользователя.", e.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        _logger.Information("Пароль пользователя успешно установлен.");
        return IdentityResult.Success;
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