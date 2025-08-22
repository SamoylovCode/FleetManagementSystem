using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Configs;
using ILogger = Serilog.ILogger;
using MailKit.Net.Smtp;
using MimeKit;

namespace FleetManagementSystemApp.Business.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger _logger;
    private readonly string _username;
    private readonly string _password;

    public EmailSender(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        _username = Environment.GetEnvironmentVariable("MAILTRAP_USERNAME");
        _password = Environment.GetEnvironmentVariable("MAILTRAP_PASSWORD");
    }

    public async Task<Result> SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_emailSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder{ HtmlBody = body };
        message.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.Log(UserServiceErrors.SendEmailFailed(e.Message), Levels.Error);
            return Result.Failure(UserServiceErrors.SendEmailFailed(e.Message));
        }
    }
}