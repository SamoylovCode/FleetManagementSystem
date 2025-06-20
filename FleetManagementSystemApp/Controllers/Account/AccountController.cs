using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Data.Repositories.Abstract;
using FleetManagementSystemApp.ViewModels;
using FleetManagementSystemApp.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;
using System.Collections.Generic;
using System.Net;
using static FleetManagementSystemApp.Common.Extensions.Levels;
using static System.Runtime.InteropServices.JavaScript.JSType;
/*Alies*/
using Err = FleetManagementSystemApp.Business.Services.Errors.UserServiceErrors;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Controllers.Account;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserService _userService;
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger _logger;

    public string ReturnUrl { get; set; }

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        RoleManager<IdentityRole> userRole,
        IUserService userService,
        IConfirmationService confirmationService,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = context;
        _roleManager = userRole;
        _userService = userService;
        _confirmationService = confirmationService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl; //Get from URL-query
        return PartialView("_LoginPartial");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_LoginPartial", model);
        }

        var loginResult = await _userService.LoginUserAsync(model);

        return loginResult.ToActionResult(
            onSuccess: () =>
            {
                if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
                {
                    _logger.Warning("Attempt to redirect to external URL: {ReturnUrl}", returnUrl);
                    returnUrl = "/Autopark/Vehicles";
                }

                return RedirectToAction("Vehicles", "Autopark");
            },
            onFailure: (errors) =>
            {
                foreach (var e in errors)
                {
                    ModelState.AddModelError(e.Code ?? string.Empty, e.UserDescription);
                }

                return PartialView("_LoginPartial", model);
            });
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl; //Get from URL-query

        return PartialView("_RegisterPartial");
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_RegisterPartial", model);
        }

        var registerResult = await _userService.CreateUserAsync(model, Request.Scheme);

        return registerResult.ToActionResult(
            onSuccess: () =>
            {
                if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
                {
                    _logger.Warning("Attempt to redirect to external URL: {ReturnUrl}", returnUrl);
                    returnUrl = "Autopark/Vehicles";
                }

                return RedirectToAction("Vehicles", "Autopark");
            },
            onFailure: (errors) =>
            {
                if(errors.Count > 0)
                {
                    foreach (var e in errors)
                    {
                        ModelState.AddModelError(e.Code ?? "", e.UserDescription);
                    }
                }

                return PartialView("_RegisterPartial", model);
            });
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.Log(Err.UserNotFoundById(userId), Warning);
            return View("Error", new ErrorViewModel { Description = Err.UserNotFoundById(userId).UserDescription!});
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

            return await confirmResult.ToActionResultAsync(
                onSuccess: async () =>
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return View("ConfirmEmailSuccess");
                },
                onFailure: async (errors) =>
                {
                    _logger.Error("Error confirming email for user {TargetUserId}. Errors: {@Errors}.", userId, errors);
                    return View("Error", new ErrorViewModel { Description = Err.EmailConfirmedFailed(userId).UserDescription! });
                });
        }
    }

    [HttpGet]
    public IActionResult SetPasswordForm(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            _logger.Error("User ID {TargetUserId} or token {Token} not provided to set password.", userId, token);
            return View("Error", new ErrorViewModel { Description = "Идентификатор пользователя или токен не переданы, чтобы назначить пароль учетной записи." });
        }

        return View("SetPassword", new SetPassword { UserId = userId, Token = token });
    }

    [HttpPost]
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

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    //[HttpPost]
    //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    //{
    //    if(!ModelState.IsValid)
    //    {
    //    }

    //    var resetResult = await _userService.ResetPasswordAsync(model);

    //    if (!resetResult.Succeeded)
    //    {
    //    }

    //    return RedirectToAction(nameof(ChangeUserPassword));
    //}

    //[HttpPatch]
    //public async Task<IActionResult> ChangeUserPassword(ChangeUserViewModel model)
    //{

    //}

    //[HttpPost]
    //[Authorize(Policy = "Admin")]
    //public async Task<IActionResult> ManageUser(ApplicationUser id)
    //{
    //    return View();
    //}
}