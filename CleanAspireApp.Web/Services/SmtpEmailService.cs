using CleanAspireApp.Domain.Common.Base;
using CleanAspireApp.Domain.ValuationRoll;
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
        email.To.Add(MailboxAddress.Parse(_configuration["EmailSettings:ToPrimary"] ?? throw new InvalidOperationException("EmailSettings:ToPrimary is not configured")));
        email.Cc.Add(MailboxAddress.Parse(_configuration["EmailSettings:ToSecondary"] ?? throw new InvalidOperationException("EmailSettings:ToSecondary is not configured")));
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

    public async Task SendReviewAsync(ContactModel contactModel, PropertyRecord propertyRecord)
    {

        //https://onecompiler.com/html/44kedefmj
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(contactModel.Email));
        email.To.Add(MailboxAddress.Parse(_configuration["EmailSettings:ToPrimary"] ?? throw new InvalidOperationException("EmailSettings:ToPrimary is not configured")));
        email.Bcc.Add(MailboxAddress.Parse(_configuration["EmailSettings:ToSecondary"] ?? throw new InvalidOperationException("EmailSettings:ToSecondary is not configured")));
        email.Subject = "Valuation review";

        var sysBody = $@"
<html>
<body style='margin:0; padding:0; background-color:#f4f4f4; font-family:Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f4f4; padding:20px;'>
<tr>
<td align='center'>

    <table width='600' cellpadding='0' cellspacing='0' 
           style='background-color:#ffffff; border-radius:8px; overflow:hidden;'>

        <!-- HEADER / LOGO -->

        <tr>
            <td align='center' style='background-color:#1a73e8; padding:20px;'>
                   <!-- Replace with your actual logo URL 
                    <img src='https://valunation.co.za/wp-content/uploads/2024/10/logo-valunation-g.png' 
                    alt='e-Roll Logo' 
                    style='max-width:150px; display:block;' />     
                   -->
            </td>
        </tr>

        <!-- TITLE -->
        <tr>
            <td style='padding:20px 20px 10px 20px; font-size:18px; font-weight:bold; color:#333;'>
                New Message from e-Roll
            </td>
        </tr>

        <!-- INTRO -->
        <tr>
            <td style='padding:0 20px 15px 20px; font-size:14px; color:#555;'>
                <strong>{contactModel.Email}</strong> sent a message from the e-Roll website.
            </td>
        </tr>

        <!-- PROPERTY INFO -->
        <tr>
            <td style='padding:0 20px 10px 20px; font-size:14px; color:#333;'>
                <strong>Property Reference:</strong><br />
                {propertyRecord.Description}
            </td>
        </tr>

        <!-- CTA BUTTON -->
        <tr>
            <td align='center' style='padding:20px;'>
                <a href='{propertyRecord.Link}' 
                   style='background-color:#f59e0b;
                          color:#ffffff;
                          text-decoration:none;
                          padding:12px 20px;
                          border-radius:5px;
                          display:inline-block;
                          font-size:14px;
                          font-weight:bold;'>
                    View Property
                </a>
            </td>
        </tr>

        <!-- FALLBACK LINK -->
        <tr>
            <td style='padding:0 20px 15px 20px; font-size:12px; color:#777; word-break:break-all;'>
                Or copy and paste this link into your browser:<br />
                {propertyRecord.Link}
            </td>
        </tr>

        <!-- MESSAGE -->
        <tr>
            <td style='padding:20px; font-size:14px; color:#333; border-top:1px solid #eee;'>
                <strong>Message:</strong><br /><br />
                <div style='background-color:#fafafa; padding:15px; border-radius:5px;'>
                    {contactModel.Message}
                </div>
            </td>
        </tr>

        <!-- FOOTER -->
        <tr>
            <td style='padding:15px; font-size:12px; color:#999; text-align:center;'>
                © e-Roll System
            </td>
        </tr>

    </table>

</td>
</tr>
</table>

</body>
</html>";

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
