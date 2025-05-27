using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Data.Repositories.Abstract;
using FleetManagementSystemApp.ViewModels;
using FleetManagementSystemApp.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

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

    public string ReturnUrl { get; set; }

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             ApplicationDbContext context,
                             RoleManager<IdentityRole> userRole,
                             IUserService userService,
                             IConfirmationService confirmationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = context;
        _roleManager = userRole;
        _userService = userService;
        _confirmationService = confirmationService;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl; //Get from URL-query
        return View("~/Views/Account/_LoginPartial.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/_LoginPartial.cshtml", model);
        }

        var loginResult = await _userService.LoginUserAsync(model);

        if (!loginResult.Succeeded)
        {
            return BadRequest(loginResult.Errors);
        }

        if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
        {
            // TODO: _logger.LogWarning($"Попытка перенаправления на внешний URL: {returnUrl}");
            returnUrl = "/";
        }

        return LocalRedirect(returnUrl ?? "/");
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

        return View("~/Views/Account/_RegisterPartial.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/_RegisterPartial.cshtml", model);
        }

        var registerResult = await _userService.CreateUserAsync(model, Request.Scheme);
        
        if (!registerResult.Succeeded)
        {
            return BadRequest(registerResult.Errors);
        }

        if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
        {
            // TODO: _logger.LogWarning($"Попытка перенаправления на внешний URL: {returnUrl}");
            returnUrl = "/";
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(string userId, string token)
    {
        var hasPassword = await _confirmationService.ConfirmAsync(userId, token);

        if (hasPassword)
        {
            return View("~/Views/Account/ConfirmEmailSuccess.cshtml");
        }
        
        return RedirectToAction("SetPasswordForm", new { userId });

        //var model = new PartialPageModel
        //{
        //    PartialViewName = "_SetPasswordPartial",
        //    ViewModel = new SetPassword { UserId = userId }
        //};

        //return View("_EmptyLayout", model);

        //return View("~/Views/Account/ConfirmEmailSuccess.cshtml");
    }

    [HttpGet]
    public IActionResult SetPasswordForm(string userId)
    {
        var model = new PartialPageModel
        {
            PartialViewName = "_SetPasswordPartial",
            ViewModel = new SetPassword { UserId = userId }
        };

        return View("_EmptyLayout", model);
        //return View("SetPassword", new SetPassword { UserId = userId });
    }

    [HttpPost]
    public async Task<IActionResult> SetPassword(SetPassword model)
    {
        if (!ModelState.IsValid)
        {
            return View("_EmptyLayout", new PartialPageModel
            {
                PartialViewName = "_SetPasswordPartial",
                ViewModel = model
            });
        }

        var result = await _userService.SetPasswordAsync(model);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return RedirectToAction("Login", "Account");
    }

    //[HttpGet]
    //public async Task<IActionResult> AccessDenied()
    //{
    //    return View();
    //}

    //[HttpPost]
    //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    //{
    //    if(!ModelState.IsValid)
    //    {
    //        return BadRequest();
    //    }

    //    var resetResult = await _userService.ResetPasswordAsync(model);

    //    if (!resetResult.Succeeded)
    //    {
    //        return BadRequest(resetResult.Errors);
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