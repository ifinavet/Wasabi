using System.ComponentModel.DataAnnotations;

namespace IfiNavet.Web.Core.ViewModels.Member;

public class MemberResetPasswordViewModel
{
    [Required]
    [StringLength(36, MinimumLength = 10, ErrorMessage = "Passordet må være melleom 10 og 36 karakterer.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passordene stemmer ikke overens med hverandre.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}