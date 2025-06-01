using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace FleetManagementSystemApp.Business.Services;

public class ConfirmationEmailService : IConfirmationService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IEmailSender _emailSender;
    private readonly LinkGenerator _linkGenerator;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmationEmailService(UserManager<ApplicationUser> userManager,
                                    ApplicationDbContext dbContext,
                                    SignInManager<ApplicationUser> signInManager,
                                    IHttpContextAccessor contextAccessor,
                                    LinkGenerator linkGenerator,
                                    IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        _linkGenerator = linkGenerator;
        _emailSender = emailSender;
    }

    public async Task<IdentityResult> SendConfirmationAsync(ApplicationUser user, string scheme)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (_contextAccessor.HttpContext is null)
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
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Письмо не отправлено.", e);
        }
        
        return IdentityResult.Success;
    }
}
