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
using Umbraco.Cms.Web.Website.Controllers;
using Wasabi.Services;
using Wasabi.ViewModels.Member;

namespace Wasabi.Controllers.Member;

public class MemberResetPasswordController : SurfaceController
{
    private readonly IEmailService _emailService;
    private readonly ILogger<MemberResetPasswordController> _logger;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;

    public MemberResetPasswordController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberService memberService,
        IEmailService emailService,
        IMemberManager memberManager,
        ILogger<MemberResetPasswordController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberService = memberService;
        _emailService = emailService;
        _memberManager = memberManager;
        _logger = logger;
    }

    /// <summary>
    ///     Handles the request to reset a member's password by generating a reset token and sending it via email.
    /// </summary>
    /// <param name="model">The view model containing the member's email address for the password reset request.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that returns the current Umbraco page with a status message indicating the result
    ///     of the request.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MemberRequestPasswordReset(MemberRequestPasswordResetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["status"] = "Ugyldig e-postadresse oppgitt.";
            return CurrentUmbracoPage();
        }

        IMember? member = _memberService.GetByEmail(model.Email);

        if (member == null)
        {
            TempData["status"] = "Ugyldig e-postadresse oppgitt.";
            return CurrentUmbracoPage();
        }

        member.SetValue("validateGUIDExpiry", DateTime.Now.AddHours(2));

        _memberService.Save(member);

        MemberIdentityUser identityUser = (await _memberManager.FindByEmailAsync(model.Email))!;
        _logger.LogInformation(identityUser.Id);
        string resetToken = await _memberManager.GeneratePasswordResetTokenAsync(identityUser);

        Dictionary<string, string> emailFields = new()
        {
            { "FIRSTNAME", member.GetValue<string>("firstName")! },
            { "LASTNAME", member.GetValue<string>("lastName")! },
            { "EMAIL", member.Email },
            { "RESETTOKEN", resetToken },
            { "DOMAIN", HttpContext.Request.Host.Value }
        };

        bool emailSent = _emailService.SendEmail("Password Reset Email", model.Email, emailFields);

        if (!emailSent)
            TempData["status"] = "Det oppsto en feil, vennligst prøv igjen senere";
        else
            TempData["status"] = "En e-post med tilbakestilling av passord er sendt til e-postadressen din.";

        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     Resets the member's password using the provided reset token and new password.
    /// </summary>
    /// <param name="model">The view model containing the new password information.</param>
    /// <param name="email">The email address of the member requesting the password reset.</param>
    /// <param name="resetToken">The token used to validate the password reset request.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that redirects to the login page if the reset is successful,
    ///     or returns the current Umbraco page with an error message if the reset fails.
    /// </returns>
    public async Task<IActionResult> MemberResetPassword(
        MemberResetPasswordViewModel model,
        [FromQuery(Name = "email")] string? email,
        [FromQuery(Name = "resetToken")] string? resetToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["status"] = "Se til at du har oppgitt all data";
            return CurrentUmbracoPage();
        }

        if (email == null && resetToken == null)
        {
            TempData["status"] = "Det har oppstått en feil - prøv å be om et nytt passord på nytt.";
            return CurrentUmbracoPage();
        }

        IdentityResult validateNewPassword = _memberManager.ValidatePasswordAsync(model.NewPassword).Result;
        if (!validateNewPassword.Succeeded)
        {
            TempData["status"] = "Passordet oppfyller ikke kravene.";
            return CurrentUmbracoPage();
        }

        IMember? member = _memberService.GetByEmail(email!);

        if (member == null)
        {
            TempData["status"] = "Ugyldig informasjon ble oppgtitt";
            return CurrentUmbracoPage();
        }

        DateTime memberValidateGuidExpiry = member.GetValue<DateTime>("validateGUIDExpiry");

        if (DateTime.Now < memberValidateGuidExpiry)
        {
            member.IsLockedOut = false;
            member.IsApproved = true;
            member.SetValue("validateGUIDExpiry", DateTime.Now.AddHours(-1));
            _memberService.Save(member);

            MemberIdentityUser? identityUser = await _memberManager.FindByIdAsync(member.Id.ToString());

            if (identityUser == null)
            {
                TempData["status"] = "Det oppsto en feil - prøv igjen senere.";
                return CurrentUmbracoPage();
            }

            IdentityResult passwordResetResult =
                _memberManager.ResetPasswordAsync(identityUser, resetToken!, model.NewPassword).Result;

            if (passwordResetResult.Succeeded) return Redirect("/login/");

            TempData["status"] = "Det oppsto en feil - prøv igjen senere.";
            return CurrentUmbracoPage();
        }

        TempData["status"] = "Koblingen din har utløpt - prøv å be om et nytt passord på nytt.";
        return CurrentUmbracoPage();
    }
}