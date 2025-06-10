using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmationEmailService(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        LinkGenerator linkGenerator,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _contextAccessor = contextAccessor;
        _linkGenerator = linkGenerator;
        _emailSender = emailSender;
    }

    public async Task<IdentityResult> SendConfirmationAsync(ApplicationUser user, string scheme)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (_contextAccessor.HttpContext is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "HttpContext не содержит информации о текущем запросе."
            });
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
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException("Письмо не отправлено.", e);
        }
        
        return IdentityResult.Success;
    }
}
