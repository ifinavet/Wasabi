using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace IfiNavet.Web.Core.ViewModels;

public class MemberLoginViewModel
{
    [Required] public string LoginName { get; set; }

    [Required] public string Password { get; set; }

    public IPublishedContent ResetPassword { get; set; }

    public IPublishedContent SignUp { get; set; }
}