using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IEmailSender
{
    Task<Result> SendEmailAsync(string toEmail, string subject, string body);
}