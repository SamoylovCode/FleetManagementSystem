using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Account;
using FleetManagementSystemApp.ViewModels.Admin;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using System.Transactions;

using static FleetManagementSystemApp.Common.Extensions.Levels;
using FleetManagementSystemApp.Business.Services.Errors;
using ILogger = Serilog.ILogger;

/*Alies*/
using Err = FleetManagementSystemApp.Business.Services.Errors.UserServiceErrors;

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
    private readonly IHybridCache _hybridCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly UserAddingValidator _userAddValidator;

    public UserService(
        ApplicationUserDtoExtentions userDto,
        UserManager<ApplicationUser> userManager,
        UserCreationValidator validator,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        UserLoginValidator loginValidator,
        SignInManager<ApplicationUser> signInManager,
        ICurrentUserService currentUserService,
        IConfirmationService confirmationEmailService,
        IHybridCache hybridCache,
        IConnectionMultiplexer redis,
        UserAddingValidator userAddValidator)
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
        _hybridCache = hybridCache;
        _redis = redis;
        _userAddValidator = userAddValidator;
    }

    private async Task<List<Claim>> CreateUserClaims(ApplicationUser user, string? role = null)
    {
        _logger.Information("Creating user account claims.");
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.GivenName, $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim()),
            new Claim("CompanyId", user.CompanyId.ToString())
        };

        if (string.IsNullOrEmpty(role))
        {
            _logger.Warning("Role not specified during claims creation.");
        }

        _logger.Information("Retrieving user {TargetUserId} roles ", user.Id);
        var userRoles = await _userManager.GetRolesAsync(user);
        userRoles = userRoles
            .Append(role)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .ToList()!;

        foreach(var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            _logger.Information("Added role {Role} to claims.", userRole);
        }

        _logger.Debug("Claims created successefully.");
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
        _logger.Information("Getting list of company {CompanyId} users.", companyId);
        if (string.IsNullOrEmpty(companyId))
        {
            _logger.Log(Err.CompanyNotFound(companyId), Warning);
            return Result<List<ApplicationUserDto>>.Failure(Err.CompanyNotFound(null));
        }
        
        // TODO: нельзя кешировать PII (DTO модель, содержащую email)
        return await _hybridCache.GetOrAddAsync(async () =>
        {
            var users = await _userManager.Users
                .Where(u => u.CompanyId == Guid.Parse(companyId))
                .AsNoTracking()
                .ToListAsync();

            if(users.Count > 0)
            {
                _logger.Information("Returned list of {UsersCount} company {CompanyId} users.", users.Count, companyId);
                return _userMapper.ToDto(users);
            }
            else
            {
                _logger.Log(Err.CompanyHasNoEmployees(companyId), Warning);
                return Result<List<ApplicationUserDto>>.Failure(Err.CompanyHasNoEmployees(companyId));
            }
        }, companyId, TimeSpan.FromMinutes(1), $"users:list");
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByIdAsync(string userId)
    {
        _logger.Information("Getting user {TargetUserId} data.", userId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.Log(Err.UserIdIsNullOrEmpty(), Warning);
            return Result<ApplicationUserDto>.Failure(Err.UserIdIsNullOrEmpty());
        }

        // TODO: нельзя кешировать PII (DTO модель, содержащую email)
        return await _hybridCache.GetOrAddAsync(async () =>
        {
            var user = await _userManager.FindByIdAsync(userId);

            if(user is not null)
            {
                _logger.Information("User {TargetUserId} is found, returned ApplicationUserDto instance.", userId);
                return _userMapper.ToDto(user);
            }
            else
            {
                _logger.Log(Err.UserNotFoundById(userId), Warning);
                return Result<ApplicationUserDto>.Failure(Err.UserNotFoundById(userId));
            }
        }, userId, TimeSpan.FromMinutes(5), "user:by-id");
    }

    public async ValueTask<Result<ApplicationUserDto>> GetUserByEmailAsync(string email)
    {
        _logger.Information("Getting user by email.");
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.Log(Err.EmailIsNullOrEmpty(), Warning);
            return Result<ApplicationUserDto>.Failure(Err.EmailIsNullOrEmpty());
        }

        // TODO: нельзя кешировать PII (email, DTO модель, содержащую email)
        return await _hybridCache.GetOrAddAsync(async () =>
        {
            var user = await _userManager.FindByEmailAsync(email);

            if(user is not null)
            {
                _logger.Information("User {TargetUserId} is found, returned ApplicationUserDto instance.");
                return _userMapper.ToDto(user);
            }
            else
            {
                var maskedEmail = email is not null && email.IndexOf('@') is int idx && idx > 0
                        ? "***" + email.Substring(idx)
                        : "***";
                _logger.Log(Err.EmailNotFound(maskedEmail), Warning);
                return Result<ApplicationUserDto>.Failure(Err.EmailNotFound(maskedEmail));
            }
        }, email, TimeSpan.FromMinutes(5), "user:by-email");
    }

    public async ValueTask<Result> LoginUserAsync(LoginViewModel model)
    {
        _logger.Information("User login.");
        var validationResult = _loginValidator.Validate(model);
        if (!validationResult.IsValid)
        {
            _logger.Warning("Login model validation failed. Error count: {ErrorCount}. Error fields: {ErrorFields}. Error messages: {ErrorMessages}.",
                validationResult.Errors.Count,
                validationResult.Errors.Select(e => e.PropertyName).Distinct(),
                validationResult.Errors.Select(e => e.ErrorMessage).Distinct());
            return Result.Failure(
                validationResult.Errors.Select(e => new Error {
                    Code = e.ErrorCode,
                    DevDescription = e.ErrorMessage
                }).ToArray());
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            var maskedEmail = model.Email is not null && model.Email.IndexOf('@') is int idx && idx > 0
                    ? "***" + model.Email.Substring(idx)
                    : "***";
            _logger.Log(Err.UserNotFoundByEmail(maskedEmail));
            return Result.Failure(Err.EmailNotFound(maskedEmail));
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!checkPassword)
        {
            _logger.Log(Err.PasswordDoesNotMatch(user.Id), Warning);
            return Result.Failure(Err.PasswordDoesNotMatch(user.Id));
        }
        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

        _logger.Information("User {TargetUserId} logged in.", user.Id);
        return Result.Success();
    }

    public async ValueTask<Result> AddUserAsync(AddUserViewModel model, string scheme)
    {
        var validationResult = _userAddValidator.Validate(model);
        if (!validationResult.IsValid)
        {
            _logger.Warning(
                "Adding user model validation failed. Error count: {ErrorCount}. Error fields: {ErrorFields}. Error messages: {ErrorMessages}.",
                validationResult.Errors.Count,
                validationResult.Errors.Select(e => e.PropertyName).Distinct(),
                validationResult.Errors.Select(e => e.ErrorMessage).Distinct());
            return Result.Failure(
                validationResult.Errors.Select(e => new Error
                {
                    Code = e.ErrorCode,
                    DevDescription = e.ErrorMessage
                }).ToArray());
        }

        var currentUserCompanyId = _currentUserService.CompanyId;
        _logger.Information("Adding user in company {CompanyId}", currentUserCompanyId);
        if (string.IsNullOrEmpty(currentUserCompanyId))
        {
            _logger.Log(Err.CompanyNotFound(currentUserCompanyId), Warning);
            return Result.Failure(Err.CompanyNotFound(null));
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

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var exectuteUserCreationResult = await ExecuteUserCreationAsync(user, null, model.Role);
            if (exectuteUserCreationResult.IsFailure)
            {
                _logger.Error(
                    "User registration failed. Error codes: {ErrorCode}. Error messages: {ErrorMessage}.",
                    exectuteUserCreationResult.Errors.Select(e => e.Code).Distinct(),
                    exectuteUserCreationResult.Errors.Select(e => e.DevDescription).Distinct());
                return Result.Failure(Err.UserCreationFailed());
            }

        scope.Complete();

        _logger.Information("Sending user {TargetUserId} registration confirmation email.", user.Id);
        var confirmEmailResult = await _confirmationEmailService.SendConfirmationAsync(user, scheme);

        if (confirmEmailResult.IsFailure)
        {
            _logger.Log(Err.SendEmailFailed(user.Id), Levels.Error);
            return Result.Failure(Err.SendEmailFailed(user.Id));
        }
        
        await _hybridCache.RemoveByPrefixAsync("users:list");

        _logger.Information("User {TargetUserId} added to company {CompanyId} successfully.", user.Id, currentUserCompanyId);
        return Result.Success();
    }

    public async ValueTask<Result> CreateUserAsync(RegisterViewModel model, string scheme)
    {
        _logger.Information("Company and user registration.");
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

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        _dbContext.Companies.Add(company);
        _dbContext.Addresses.Add(address);
        await _dbContext.SaveChangesAsync();

        var executeUserCreationResult = await ExecuteUserCreationAsync(user, model.Password, ApplicationRole.Admin);
        if (executeUserCreationResult.IsFailure)
        {
            _logger.Error(
                "User registration failed. Error codes: {ErrorCode}. Error messages: {ErrorMessage}.",
                executeUserCreationResult.Errors.Select(e => e.Code).Distinct(),
                executeUserCreationResult.Errors.Select(e => e.DevDescription).Distinct());
            return Result.Failure(Err.UserCreationFailed());
        }

        scope.Complete();

        _logger.Information("Sending user {TargetUserId} registration confirmation email.", user.Id);
        var confirmEmailResult = await _confirmationEmailService.SendConfirmationAsync(user, scheme);

        if (confirmEmailResult.IsFailure)
        {
            _logger.Log(Err.SendEmailFailed(user.Id), Warning);
            return Result.Failure(Err.SendEmailFailed(user.Id));
        }

        _logger.Information("User {TargetUserId} registered successfully.", user.Id);
        return Result.Success();
    }

    private async Task<Result> ExecuteUserCreationAsync(ApplicationUser user, string? password, string role)
    {
        _logger.Information("Executing user registration.");
        var validator = _serviceProvider.GetRequiredService<IValidator<(ApplicationUser user, string? password, string role)>>();
        var validationResult = await _validator.ValidateAsync((user, password, role));
        if (!validationResult.IsValid)
        {
            _logger.Warning("Login model validation failed. Error count: {ErrorCount}. Error fields: {ErrorFields}. Error messages: {ErrorMessages}",
                validationResult.Errors.Count,
                validationResult.Errors.Select(e => e.PropertyName).Distinct(),
                validationResult.Errors.Select(e => e.ErrorMessage).Distinct());
            return Result.Failure(
                validationResult.Errors.Select(e => new Error
                {
                   DevDescription = e.ErrorMessage
                }).ToArray());
        }

        var createResult = string.IsNullOrEmpty(password)
            ? await _userManager.CreateAsync(user)
            : await _userManager.CreateAsync(user, password);
        _logger.Information("Specified user creation attempt.");

        if (!createResult.Succeeded)
        {
            _logger.Log(Err.UserCreationFailed(), Levels.Error);
            return Result.Failure(Err.UserCreationFailed());
        }

        _logger.Information("Adding role to user {TargetUserId}", user.Id);
        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            _logger.Log(Err.AddToRoleFailed(user.Id), Levels.Error);
            return Result.Failure(Err.AddToRoleFailed(user.Id));
        }

        var claims = await CreateUserClaims(user, role);
        _logger.Information("Adding created claims to user {TargetUserId} account.", user.Id);
        var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
        if (!addClaimsResult.Succeeded)
        {
            _logger.Log(Err.AddClaimsFailed(user.Id), Warning);
            return Result.Failure(Err.AddClaimsFailed(user.Id));
        }

        _logger.Information("User registration completed successfully.");
        return Result.Success();
    }

    public async ValueTask<Result> SetPasswordAsync(SetPassword model)
    {
        _logger.Information("Setting password for user account.");

        if (model.UserId is null)
        {
            _logger.Log(Err.UserIdIsNullOrEmpty(), Warning);
            return Result.Failure(Err.UserIdIsNullOrEmpty());
        }

        _logger.Information("Searching user {TargetUserId} by ID.", model.UserId);
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            _logger.Log(Err.UserNotFoundById(model.UserId), Warning);
            return Result.Failure(Err.UserNotFoundById(model.UserId));
        }

        _logger.Information("Confirmation email.");

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var emailConfirmed = await _userManager.ConfirmEmailAsync(user, model.Token);
        if (!emailConfirmed.Succeeded)
        {
            _logger.Log(Err.EmailConfirmedFailed(user.Id), Warning);
            return Result.Failure(Err.EmailConfirmedFailed(user.Id));
        }

        _logger.Information("Adding password to user {TargetUserId} account.", user.Id);
        var resultAddPassword = await _userManager.AddPasswordAsync(user, model.Password);
        if (!resultAddPassword.Succeeded)
        {
            _logger.Log(Err.AddPasswordFailed(user.Id), Warning);
            return Result.Failure(Err.AddPasswordFailed(user.Id));
        }

        scope.Complete();

        _logger.Information("User {TargetUserId} set password successully.", user.Id);

        return Result.Success();
    }

    //public async ValueTask<Result> UpdateUserDataAsync(UpdateUserDataViewModel model)
    //{
    //    await UpdateUserClaimsAsync(user);
    //    //Do something
    //}

    //public ValueTask<Result> ResetUserPasswordAsync(ResetPasswordViewModel model)
    //{

    //}
}