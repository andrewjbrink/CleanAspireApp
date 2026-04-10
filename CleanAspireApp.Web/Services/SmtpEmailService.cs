using CleanAspireApp.Web.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace CleanAspireApp.Web.Services;

public class SmtpEmailService : IEmailService
{

    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> SendEmailAsync(string from, string body)
    {
        throw new NotImplementedException();
    }

    public async Task SendClientEmailAsync(string from, string body)
    {

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from));
        email.To.Add(MailboxAddress.Parse(_configuration["EmailSettings:To"] ?? throw new InvalidOperationException("EmailSettings:To is not configured")));
        email.Subject = "Roll Enquiry";

        var sysBody = $"{from} sent a message from the e-Roll website." +
                          "<br />" +
                          "<br />" +
                          $"{body}";

        var sysBodyBuilder = new BodyBuilder();
        sysBodyBuilder.HtmlBody = sysBody;
        email.Body = sysBodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _configuration["EmailSettings:SmtpHost"],
            int.Parse(_configuration["EmailSettings:Port"]!),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"]);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
