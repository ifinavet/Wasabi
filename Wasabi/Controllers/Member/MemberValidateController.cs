using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Wasabi.Controllers.Member;

public class MemberValidateController : RenderController
{
    private readonly IMemberService _memberService;
    public MemberValidateController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _memberService = memberService;
    }

    /// <summary>
    /// Validates the member's email address using the provided email and validation GUID.
    /// </summary>
    /// <param name="email">The email address of the member to be validated.</param>
    /// <param name="validateGuid">The GUID used to validate the member's email address.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns the current Umbraco page with a status message indicating the result of the validation.
    /// </returns>
    [HttpGet]
    [Route("/profil/validate/")]
    public IActionResult Index(
        [FromQuery(Name = "email")] string? email,
        [FromQuery(Name = "validateGUID")] string? validateGuid)
    {
        if (email == null || validateGuid == null)
        {
            TempData["Success"] = false;
            TempData["status"] = "Feil ved validering av e-postadressen din";
            return CurrentTemplate(CurrentPage);
        }

        IMember member = _memberService.GetByEmail(email)!;
        string memberValidateGuid = member.GetValue<string>("validateGUID")!.ToLower();
        DateTime memberValidateGuidExpiry = member.GetValue<DateTime>("validateGUIDExpiry");

        if (memberValidateGuid == validateGuid &&
            memberValidateGuidExpiry > DateTime.Now)
        {
            member.IsApproved = true;
            member.SetValue("validateGUIDExpiry", DateTime.Now.AddDays(-1));
            _memberService.Save(member);
            TempData["Success"] = true;
            TempData["status"] = "Kontoen din er blitt validert - du kan nå logge inn.";
        }
        else
        {
            TempData["Success"] = false;
            TempData["status"] =
                "Beklager - det ser ikke ut til at vi kan validere e-postadressen din. " +
                "Prøv å bruke funksjonen for glemt passord for å tilbakestille kontoen din.";
        }

        return CurrentTemplate(CurrentPage);
    }
}