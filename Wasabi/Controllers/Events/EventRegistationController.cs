using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;
using Wasabi.Helpers;
using Wasabi.ViewModels.Events;

namespace Wasabi.Controllers.Events;

public class EventRegistrationController : SurfaceController
{
    private readonly IMemberManager _memberManger;
    private readonly IMemberService _memberService;
    private readonly IContentService _contentService;

    public EventRegistrationController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManger,
        IMemberService memberService,
        IContentService contentService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManger = memberManger;
        _memberService = memberService;
        _contentService = contentService;
    }

    public async Task<IActionResult> Register(EventRegistrationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        MemberIdentityUser? currentMember = await _memberManger.GetCurrentMemberAsync();
        IMember currentStudent = _memberService.GetByKey(currentMember!.Key)!;
        if (!Studieprogram.StudieprogramList.Contains(currentStudent.GetValue<string>("studieprogram") ?? string.Empty))
        {
            TempData["status"] = "Du har ikke valgt et gyldig studieprogram";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        if (currentStudent.GetValue<int>("semester") < 1)
        {
            TempData["status"] = "Ugyldig semester verdi";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        if (CurrentPage is not Event currentPage)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        IContent newAttendee = _contentService.Create(currentMember.Name!, currentPage.Id, Attendee.ModelTypeAlias);

        newAttendee.SetValue("attendingMember", new GuidUdi("member", currentMember.Key));
        newAttendee.SetValue("shownUp", false);
        newAttendee.SetValue("allergies", model.Allergie);
        newAttendee.SetValue("memberId", currentMember.Key.ToString());

        PublishResult publishResult = _contentService.SaveAndPublish(newAttendee);
        if (!publishResult.Success)
        {
            TempData["status"] = "Kunne ikke melde deg på arrangementet, prøv igjen senere";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        return CurrentUmbracoPage();
    }

    public IActionResult Unregister()
    {
        if (CurrentPage is not Event currentPage)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage();
        }

        MemberIdentityUser? currentMember = _memberManger.GetCurrentMemberAsync().Result;
        int attendeeId = currentPage.Children<Attendee>()!
            .FirstOrDefault(a => a.MemberId == currentMember!.Key.ToString())!.Id;

        IContent content = _contentService.GetById(attendeeId)!;
        OperationResult result = _contentService.Delete(content);
        if (!result.Success)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage();
        }

        return CurrentUmbracoPage();
    }
}