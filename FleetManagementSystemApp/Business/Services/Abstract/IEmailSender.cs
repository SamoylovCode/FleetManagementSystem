namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}