namespace CleanAspireApp.Domain.Common.Base;

public class EmailSettings
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string ToPrimary { get; set; } = string.Empty;
    public string ToSecondary { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int Port { get; set; }
}
