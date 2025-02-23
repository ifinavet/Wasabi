using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Wasabi.Services;
using Wasabi.ViewModels.Member;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Wasabi.Controllers.Member;

public class MemberLoginController : SurfaceController
{
    private readonly IGoogleReCaptchaService _googleReCaptchaService;
    private readonly IMemberService _memberService;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly IPointsService _pointsService;

    public MemberLoginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberService memberService,
        IMemberSignInManager memberSignInManager,
        IGoogleReCaptchaService googleReCaptchaService,
        IPointsService pointsService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberSignInManager = memberSignInManager;
        _memberService = memberService;
        _googleReCaptchaService = googleReCaptchaService;
        _pointsService = pointsService;
    }

    /// <summary>
    ///     Logs member inn
    ///     Checks if member is approved
    ///     Checks if member is banned
    /// </summary>
    /// <param name="model">Model data from the from</param>
    /// <param name="redirectUrl">Url to page user arrived from</param>
    /// <returns>Returns member to the page they arrived from if successful logg inn</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(MemberLoginViewModel model,
        [FromQuery(Name = "redirectURL")] string redirectUrl = "/")
    {
        if (!ModelState.IsValid || model.CaptchaToken is null) return CurrentUmbracoPage();

        // Validates reCaptcha
        bool captchaValid = await _googleReCaptchaService.VerifyCaptcha(model.CaptchaToken);
        if (!captchaValid)
        {
            TempData["status"] =
                "Captcha verifisering mislyktest. " +
                "Dersom du ikke er en HackerBoi! - Last inn siden på nytt eller ta kontakt med IFI-Navet.";
            return CurrentUmbracoPage();
        }

        string username = model.LoginName.Split("@").First();

        SignInResult login = await _memberSignInManager.PasswordSignInAsync(username, model.Password, true, true);

        if (login.IsNotAllowed)
        {
            TempData["status"] =
                "Før du kan logge inn, må du bekrefte e-postadressen din - " +
                "sjekk e-posten din for instruksjoner om hvordan du gjør dette. " +
                "Hvis du ikke finner denne e-posten, " +
                "kan du bruke funksjonen for glemt passord for å motta en ny e-post.";
            return CurrentUmbracoPage();
        }

        if (!login.Succeeded)
        {
            TempData["status"] = "Brukernavn eller passord er feil!";
            return CurrentUmbracoPage();
        }

        IMember currentMember = _memberService.GetByUsername(model.LoginName.Split("@").First())!;
        if (currentMember.GetValue<bool>("isBanned"))
        {
            TempData["status"] =
                "Din bruker er blitt utestengt. " +
                "Du vil ha fått en e-post fra IFI-Navet sitt styre for hvorfor dette har skjedd.";
            return CurrentUmbracoPage();
        }

        // Checks if there are points that are more than 6 months. if there is we remove them.
        List<PointEntry>? pointEntries = _pointsService.ParsedPointEntries(currentMember.GetValue<string>("points"));
        if (pointEntries == null) return Redirect(redirectUrl);

        IEnumerable<PointEntry> expiredPoints = pointEntries.ToList()
            .Where(entry => (entry.Date.AddMonths(6) - DateTime.Now).Days <= 0);
        foreach (PointEntry expiredPoint in expiredPoints) pointEntries.Remove(expiredPoint);

        string updatedPoints = string.Join(Environment.NewLine,
            pointEntries.Select(p => JsonSerializer.Serialize(p)));
        currentMember.SetValue("points", updatedPoints);
        _memberService.Save(currentMember);

        return Redirect(redirectUrl);
    }
}