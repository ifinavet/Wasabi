using System.ComponentModel.DataAnnotations;

namespace Wasabi.ViewModels.Member;

public class MemberLoginViewModel
{
    [Required(ErrorMessage = "E-post adressen eller brukernavnet mangler.")]
    public string LoginName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passordet mangler")]
    public string Password { get; set; } = string.Empty;

    public string? CaptchaToken { get; set; }
}