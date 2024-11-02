using System.ComponentModel.DataAnnotations;
using static System.String;

namespace IfiNavet.Web.Core.ViewModels;

public class MemberRegisterViewModel
{
    [Required(ErrorMessage = "Fornavn mangler")]
    public string FirstName { get; set; } = Empty;

    [Required(ErrorMessage = "Etternavn mangler")]
    public string LastName { get; set; } = Empty;

    [Required(ErrorMessage = "E-post adresse mangler")]
    [EmailAddress]
    public string Email { get; set; } = Empty;

    [Required(ErrorMessage = "Passord mangler")]
    [StringLength(36, MinimumLength = 10, ErrorMessage = "Passordet må være melleom 10 og 36 krakterer")]
    public string Password { get; set; } = Empty;

    [Required(ErrorMessage = "Gjenta passord mangler")]
    [Compare("Password", ErrorMessage = "Passordene stemmer ikke overens med hverandre.")]
    public string ConfirmPassword { get; set; } = Empty;

    [Required(ErrorMessage = "Studieprogram mangler")]
    public string StudyProgram { get; set; } = Empty;

    [Required(ErrorMessage = "Semester mangler")]
    public int Semester { get; set; }

    [Required(ErrorMessage = "Foretrukket språk mangler")]
    public string PreferredLanguage { get; set; } = Empty;
}