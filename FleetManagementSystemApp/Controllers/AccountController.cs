using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.ViewModels;
using FleetManagementSystemApp.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static FleetManagementSystemApp.Common.Extensions.Levels;

using ILogger = Serilog.ILogger;

/*Aliases*/
using ErCodes = FleetManagementSystemApp.Business.Services.Errors.UserServiceErrors;

namespace FleetManagementSystemApp.Controllers;

[AllowAnonymous]
[Route("account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;
    private readonly ILogger _logger;

    public string ReturnUrl { get; set; }

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserService userService,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var loginResult = await _userService.LoginUserAsync(model);

        return loginResult.ToActionResult(
            onSuccess: () =>
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    _logger.Warning("Invalid or missing returnUrl: {ReturnUrl}", returnUrl);
                    returnUrl = "/vehicles";
                }
                if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
                {
                    _logger.Warning("Attempt to redirect to external URL: {ReturnUrl}", returnUrl);
                    returnUrl = "/vehicles";
                }

                return Redirect(returnUrl!);
            },
            onFailure: (errors) =>
            {
                _logger.Error("User authentication error. Errors: {@Errors}", errors);
                foreach (var e in errors)
                {
                    ModelState.AddModelError(string.Empty, e.UserDescription ?? "");
                }
                return View("Login", model);
            });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet("register")]
    public IActionResult Register(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var registerResult = await _userService.CreateUserAsync(model, Request.Scheme);

        return registerResult.ToActionResult(
            onSuccess: () =>
            {
                if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
                {
                    _logger.Warning("Attempt to redirect to external URL: {ReturnUrl}", returnUrl);
                    returnUrl = "Vehicles/List";
                }

                return Redirect(returnUrl!);
            },
            onFailure: (errors) =>
            {
                _logger.Error("User registration error. Errors: {@Errors}", errors);
                foreach (var e in errors)
                {
                    ModelState.AddModelError(e.Code ?? "", e.UserDescription!);
                }

                return View(model);
            });
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm(string userId, string token)
    {
        // TODO: инкапсулировать логику в сервис, оставив контролер "тонким"

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.Log(ErCodes.UserNotFoundById(userId), Warning);
            return View("Error", new ErrorViewModel { Description = ErCodes.UserNotFoundById(userId).UserDescription!});
        }

        if(user.PasswordHash is null)
        {
            return RedirectToAction("SetPasswordForm", new { userId, token });
        }

        if (user.EmailConfirmed)
        {
            _logger.Warning("User {TargetUserId} email is already confirmed.", userId);
            return View("Error", new ErrorViewModel { Description = "Почта пользователя уже подтверждена." });
        }
        else
        {
            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);

            return await confirmResult.ToActionIdentityResultAsync(
                onSuccess: async () =>
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return View("ConfirmEmailSuccess");
                },
                onFailure: async (errors) =>
                {
                    _logger.Error("Error confirming email for user {TargetUserId}. Errors: {@Errors}.", userId, errors);
                    return View("Error", new ErrorViewModel { Description = ErCodes.EmailConfirmedFailed(userId).UserDescription! });
                });
        }
    }

    [HttpGet("set-password")]
    public IActionResult SetPasswordForm(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            _logger.Error("User ID {TargetUserId} or token {Token} not provided to set password.", userId, token);
            return View("Error", new ErrorViewModel
            {
                Description = "Идентификатор пользователя или токен не переданы, чтобы назначить пароль учетной записи."
            });
        }

        return View("SetPassword", new SetPassword { UserId = userId, Token = token });
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword(SetPassword model)
    {
        if (!ModelState.IsValid)
        {
            return View("SetPassword", model);
        }

        var result = await _userService.SetPasswordAsync(model);

        return result.ToActionResult(
            onSuccess: () =>
            {
                return View("ConfirmEmailSuccess");
            },
            onFailure: (errors) =>
            {
                if (errors.Any())
                {
                    foreach (var e in errors)
                    {
                        ModelState.AddModelError(e.Code ?? "", e.UserDescription);
                    }
                }
                return View("SetPassword", model);
            });
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}