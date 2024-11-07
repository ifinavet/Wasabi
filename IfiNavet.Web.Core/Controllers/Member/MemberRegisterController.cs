using IfiNavet.Web.Core.Services;
using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace IfiNavet.Web.Core.Controllers.Member;

public class MemberRegisterController : SurfaceController
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IEmailService _emailService;

    public MemberRegisterController(
        IMemberManager memberManager,
        IMemberService memberService,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ICoreScopeProvider coreScopeProvider,
        IEmailService emailService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _coreScopeProvider = coreScopeProvider;
        _emailService = emailService;
    }

    /// <summary>
    /// </summary>
    /// <param name="model">Register member model.</param>
    /// <returns>Result of registration operation.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(MemberRegisterViewModel model)
    {
        const string successMessage =
            "Kontoen din er opprettet, før du logger inn, vennligst sjekk e-posten din og klikk på lenken for å validere kontoen din og fullføre registreringsprosessen.";

        if (!ModelState.IsValid)
        {
            TempData["Status"] = "Se til at du har oppgitt all nødvendig informasjon.";
            return CurrentUmbracoPage();
        }


        string[] user = model.Email.Split("@");
        string username = user.First();
        string domain = user.Last();

        if ((_memberService.GetByEmail(model.Email) ?? _memberService.GetByUsername(username)) != null)
        {
            // TODO! Send new email to user to verify account
            TempData["status"] = successMessage;
            return RedirectToCurrentUmbracoPage();
        }

        if (domain != "uio.no" && !domain.EndsWith(".uio.no") && domain != "ifinavet.no")
        {
            TempData["Status"] = "Du må registrere deg med en UiO e-post.";
            return CurrentUmbracoPage();
        }

        MemberIdentityUser identityUser = MemberIdentityUser.CreateNew(
            username,
            model.Email,
            "studentMember",
            false,
            model.FirstName + " " + model.LastName);
        IdentityResult identityResult = await _memberManager.CreateAsync(
            identityUser,
            model.Password);

        if (identityResult.Succeeded)
        {
            using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            
            IMember member = _memberService.GetByKey(identityUser.Key)!;

            string newUserGuid = Guid.NewGuid().ToString("N");
            
            member.SetValue("firstName", model.FirstName);
            member.SetValue("lastName", model.LastName);
            member.SetValue("validateGUID", newUserGuid);
            member.SetValue("semester", model.Semester);
            member.SetValue("studieprogram", model.StudyProgram);
            member.SetValue("preferredLanguage", model.PreferredLanguage);
            member.SetValue("validateGUIDExpiry", DateTime.Now.AddDays(1));

            _memberService.AssignRole(member.Id, "WebsiteRegistrations");

            if (domain == "ifinavet.no")
                _memberService.AssignRole(member.Id, "NavetEventAdmins");

            _memberService.Save(member);
            
            // Set up the info for the validation email
            Dictionary<string, string> emailFields = new()
            {
                {"FIRSTNAME", model.FirstName},
                {"LASTNAME", model.LastName},
                {"EMAIL", model.Email},
                {"VALIDATEGUID", newUserGuid},
                {"DOMAIN", HttpContext.Request.Host.Value}
            };
            
            // Sends verification email to registrant
            bool emailSent = _emailService.SendEmail("Validate Registration Email", model.Email, emailFields);
            TempData["success"] = emailSent;
            
            if (!emailSent)
            {
                TempData["status"] = "Det oppsto en feil, vennligst prøv igjen senere";
                _memberService.Delete(member); // Delete the member to mitigate errors.
                return RedirectToCurrentUmbracoPage();
            }

            TempData["status"] = successMessage;
            return RedirectToCurrentUmbracoPage();
        }

        TempData["Status"] = "Se til at du har oppgitt all nødvendig informasjon.";
        return CurrentUmbracoPage();
    }
}