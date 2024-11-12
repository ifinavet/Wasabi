using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace IfiNavet.Web.Core.ViewModels.Member;

public class MemberLoginViewModel
{
    [Required(ErrorMessage = "E-post adressen eller brukernavnet mangler.")]
    public string LoginName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passordet mangler")]
    public string Password { get; set; } = string.Empty;

    public IPublishedContent ResetPassword { get; set; }

    public IPublishedContent SignUp { get; set; }
}