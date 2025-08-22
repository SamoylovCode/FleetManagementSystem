using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

using ILogger = Serilog.ILogger;

/*Aliases*/
using ErCodes = FleetManagementSystemApp.Business.Services.Errors.UserServiceErrors;

namespace FleetManagementSystemApp.Business.Services;

public class ConfirmationEmailService : IConfirmationService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IEmailSender _emailSender;
    private readonly LinkGenerator _linkGenerator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger _logger;

    public ConfirmationEmailService(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        LinkGenerator linkGenerator,
        IEmailSender emailSender,
        ILogger logger)
    {
        _userManager = userManager;
        _contextAccessor = contextAccessor;
        _linkGenerator = linkGenerator;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result> SendConfirmationAsync(ApplicationUser user, string scheme)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (_contextAccessor.HttpContext is null)
        {
            return Result.Failure(new Error(devDesc: "HttpContext не содержит информации о текущем запросе."));
        }

        var callbackUrl = _linkGenerator.GetUriByAction(
            httpContext: _contextAccessor.HttpContext,
            action: "Confirm",
            controller: "Account",
            values: new { userId = user.Id, token },
            scheme: scheme);

        var subject = "Подтверждение регистрации";
        var body = $"<p>Для подтверждения регистрации перейдите по <a href=\"{callbackUrl}\">ссылке</a>.</p>";

        var result = await _emailSender.SendEmailAsync(user.Email!, subject, body);

        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(ErCodes.SendEmailFailed(string.Empty));
    }
}