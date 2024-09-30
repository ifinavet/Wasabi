using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace IfiNavet.Web.Core.Controllers.Member;

public class MemberLoginController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly UmbracoHelper _umbracoHelper;

    public MemberLoginController(
        IMemberManager memberManager,
        IMemberService memberService,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        UmbracoHelper umbracoHelper,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager memberSignInManager)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _memberSignInManager = memberSignInManager;
        _umbracoHelper = umbracoHelper;
    }

    /// <summary>
    ///     Logs member inn
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Returns member to front page if successful logg inn</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(MemberLoginViewModel model,
        [FromQuery(Name = "redirectURL")] string redirectURL)
    {
        if (!ModelState.IsValid) return CurrentUmbracoPage();

        var member = _memberService.GetByEmail(model.LoginName) ?? _memberService.GetByUsername(model.LoginName);

        switch (member)
        {
            case null when model.Password == null:
                TempData["Status"] = _umbracoHelper.GetDictionaryValue("memberInvalidLogin");
                return CurrentUmbracoPage();
            case { IsApproved: false }:
                TempData["Status"] =
                    "Før du kan logge inn, må du bekrefte e-postadressen din - sjekk e-posten din for instruksjoner om hvordan du gjør dette. Hvis du ikke finner denne e-posten, kan du bruke funksjonen for glemt passord for å motta en ny e-post.";
                return RedirectToCurrentUmbracoPage();
        }

        var login = await _memberSignInManager.PasswordSignInAsync(member!.Username, model.Password, true, false);

        if (login.Succeeded) return Redirect(redirectURL);

        TempData["Status"] = _umbracoHelper.GetDictionaryValue("memberInvalidLogin");
        return CurrentUmbracoPage();
    }
}