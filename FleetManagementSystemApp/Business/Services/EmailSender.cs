using MailKit.Net.Smtp;
using FleetManagementSystemApp.Configs;
using MimeKit;
using FleetManagementSystemApp.Business.Services.Abstract;

namespace FleetManagementSystemApp.Business.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly string _username;
    private readonly string _password;

    public EmailSender(IConfiguration configuration)
    {
        _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        _username = Environment.GetEnvironmentVariable("MAILTRAP_USERNAME");
        _password = Environment.GetEnvironmentVariable("MAILTRAP_PASSWORD");

        //foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
        //{
        //    Console.WriteLine($"{env.Key} = {env.Value}");
        //}
        //Console.WriteLine($"MAILTRAP_USERNAME = {_username ?? "NULL"}");
        //Console.WriteLine($"MAILTRAP_PASSWORD = {_password ?? "NULL"}");
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
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
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка при отправке письма: " + e.Message);
            throw;
        }

    }
}