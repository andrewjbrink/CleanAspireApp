using System.ComponentModel.DataAnnotations;

namespace CleanAspireApp.Domain.Common.Base;

public class ContactModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string ContactNumber { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    [Required]
    public int? CaptchaAnswer { get; set; }
}
