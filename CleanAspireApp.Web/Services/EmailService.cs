using CleanAspireApp.Domain.Common.Base;
using CleanAspireApp.Domain.ValuationRoll;
using CleanAspireApp.Web.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;


namespace CleanAspireApp.Web.Services;

public class EmailService : IEmailService
{
    public Task SendClientEmailAsync(string from, string body)
    {
        throw new NotImplementedException();
    }

    //https://github.com/jstedfast/MailKit
    public async Task<string> SendEmailAsync(string from, string body)
    {
        var adminEmailToSend = new MimeMessage();
        adminEmailToSend.From.Add(MailboxAddress.Parse(from));
        adminEmailToSend.To.Add(MailboxAddress.Parse("andrew@droneview.co.za"));
        adminEmailToSend.Subject = "message from website";
        var sysBody = $"A new user just registered." +
                          "<br />" +
                          "<br />" +
                          $"{body}";

        var sysBodyBuilder = new BodyBuilder();
        sysBodyBuilder.HtmlBody = sysBody;
        adminEmailToSend.Body = sysBodyBuilder.ToMessageBody();
        var client = new SmtpClient();

        client.CheckCertificateRevocation = false;
        client.Connect("smtpauth.mweb.co.za", 587, MailKit.Security.SecureSocketOptions.StartTls);
        var response = await client.SendAsync(adminEmailToSend);
        client.Disconnect(true);
        return await Task.FromResult(response.ToString());
    }

    public Task SendReviewAsync(ContactModel contactModel, PropertyRecord propertyRecord)
    {
        throw new NotImplementedException();
    }
}