using System.ComponentModel.DataAnnotations;

namespace IfiNavet.Web.Core.ViewModels.Member;

public class MemberEditProfileFormViewModel
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Passordet stemmer ikke overens")]
    public string? ConfirmNewPassword { get; set; }

    [Required] public string StudyProgram { get; set; }
    [Required] public int Semester { get; set; }
    [Required] public string PreferredLanguage { get; set; } = "norwegian";
}