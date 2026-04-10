namespace CleanAspireApp.Web.Interfaces;

public interface IEmailService
{
    Task<string> SendEmailAsync(string from, string body);
    Task SendClientEmailAsync(string from, string body);
}
