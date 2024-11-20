using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using Wasabi.ViewModels.Member;

namespace Wasabi.Controllers.Member;

public class MemberEditProfileController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;

    public MemberEditProfileController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManager,
        IMemberService memberService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
    }
    
    /// <summary>
    /// Updates the member profile with the provided information from the form model.
    ///
    /// If member has provided new password information the password will be updated.
    /// </summary>
    /// <param name="model">The view model containing the member profile information.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that redirects to the current Umbraco page if the update is successful,
    /// or returns the current Umbraco page with an error message if the update fails.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> UpdateMember(MemberEditProfileFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Status"] = "Se til at du har oppgitt all nødvendig informasjon.";
            return CurrentUmbracoPage();
        }

        MemberIdentityUser currentMemberIdentity = (await _memberManager.GetCurrentMemberAsync())!;

        IMember currentMember = _memberService.GetByKey(currentMemberIdentity.Key)!;

        currentMember.SetValue("firstName", model.FirstName);
        currentMember.SetValue("lastName", model.LastName);
        currentMember.SetValue("studieprogram", model.StudyProgram);
        currentMember.SetValue("semester", model.Semester);
        currentMember.SetValue("preferredLanguage", model.PreferredLanguage);

        _memberService.Save(currentMember);
        TempData["status"] = "Kontoen din har blitt oppdatert.";

        // Update password
        if (!model.NewPassword.IsNullOrWhiteSpace() &&
            !model.OldPassword.IsNullOrWhiteSpace() &&
            !model.ConfirmNewPassword.IsNullOrWhiteSpace())
        {
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                TempData["status"] = "Det nye passordet stemmer ikke overens.";
                return CurrentUmbracoPage();
            }

            if (model.NewPassword == model.OldPassword)
            {
                TempData["status"] = "Det nye passordet kan ikke være det samme som det gammle";
                return CurrentUmbracoPage();
            }

            IdentityResult validNewPassword = await _memberManager.ValidatePasswordAsync(model.NewPassword);
            if (!validNewPassword.Succeeded)
            {
                TempData["status"] =
                    "Det nye passordet møter ikke kravene for passord. Se til at det er mellom 10 og 36 tegn.";
                return CurrentUmbracoPage();
            }

            IdentityResult updatePassword =
                await _memberManager.ChangePasswordAsync(currentMemberIdentity, model.OldPassword, model.NewPassword);
            if (!updatePassword.Succeeded)
            {
                TempData["status"] = "Det oppsto en feil! Kunne ikke oppdatere passordet - Prøv igjen senere.";
                return CurrentUmbracoPage();
            }

            TempData["status"] += "\n Passordet er blitt oppdatert";
        }

        return RedirectToCurrentUmbracoPage();
    }
}