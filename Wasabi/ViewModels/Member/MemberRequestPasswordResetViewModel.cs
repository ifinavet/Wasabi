using System.ComponentModel.DataAnnotations;
using static System.String;

namespace Wasabi.ViewModels.Member;

public class MemberRequestPasswordResetViewModel
{
    [Required] [EmailAddress] public string Email { get; set; } = Empty;
}