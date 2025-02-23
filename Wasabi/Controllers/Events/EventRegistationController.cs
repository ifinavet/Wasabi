using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
using Wasabi.Services;
using Wasabi.ViewModels.Events;

namespace Wasabi.Controllers.Events;

public class EventRegistrationController : SurfaceController
{
    private readonly IContentService _contentService;
    private readonly ILogger<EventRegistrationController> _logger;
    private readonly IMemberManager _memberManger;
    private readonly IMemberService _memberService;
    private readonly IPointsService _pointsService;

    public EventRegistrationController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManger,
        IMemberService memberService,
        IContentService contentService,
        ILogger<EventRegistrationController> logger,
        IPointsService pointsService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManger = memberManger;
        _memberService = memberService;
        _contentService = contentService;
        _logger = logger;
        _pointsService = pointsService;
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

        List<PointEntry>? pointEntries = _pointsService.ParsedPointEntries(currentStudent.GetValue<string>("points"));
        int? numOfPoints = pointEntries?.Select(entry => entry.Severity).Sum();
        if (numOfPoints >= 3)
        {
            _logger.LogWarning("Member have to many points");
            TempData["status"] = "Du har for mange prikker til å kunne melde deg på dette arrangementet";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        if (CurrentPage is not Event || CurrentPage == null)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        Event currentPage = (Event)CurrentPage;

        if (currentPage.Children<Attendee>()!.Any(a => a.MemberId == currentMember.Key.ToString()))
        {
            TempData["status"] = "Du er allerede registert";
            return RedirectToCurrentUmbracoPage();
        }

        if (currentPage.ParticipantLimit <= currentPage.Children<Attendee>()!.Count())
        {
            TempData["status"] = "Arrangementet er fullt";
            TempData["error"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        IContent newAttendee = _contentService.Create(currentMember.Name!, currentPage.Id, Attendee.ModelTypeAlias);
        newAttendee.SetValue("attendingMember", new GuidUdi("member", currentMember.Key));
        newAttendee.SetValue("shownUp", "");
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

        MemberIdentityUser? currentMemberIdentity = _memberManger.GetCurrentMemberAsync().Result;
        if (currentMemberIdentity == null)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage();
        }

        int attendeeId = currentPage.Children<Attendee>()!
            .FirstOrDefault(a => a.MemberId == currentMemberIdentity.Key.ToString())!.Id;

        IContent content = _contentService.GetById(attendeeId)!;
        OperationResult result = _contentService.Delete(content);
        if (!result.Success)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage();
        }

        Event? currentEvent = (Event)CurrentPage;
        int timeRemaining = (DateTime.Now - currentEvent.EventDate).Hours;
        _logger.LogDebug("Current member ${currentMember}", currentMemberIdentity.Id);

        if (timeRemaining < 24)
        {
            IMember currentMember = _memberService.GetByKey(currentMemberIdentity.Key)!;
            _pointsService.GivePoints(currentMember, "Du meldte deg av for sent", 1);

            _logger.LogWarning("Member given point for late unregistration");
        }

        return CurrentUmbracoPage();
    }

    public IActionResult RegisterAttendee(EventRegisterAttendeeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        if (CurrentPage is not Event currentPage)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        if (currentPage.Children<Attendee>().IsNullOrEmpty())
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        Attendee? attendee = currentPage.Children<Attendee>()!.FirstOrDefault(a => a.Id == int.Parse(model.AttendeeId));
        if (attendee == null)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        if (attendee.ShownUp?.ToLower() == "yes" || attendee.ShownUp?.ToLower() == "late")
        {
            TempData["status"] = "Den oppmeldte er allerede registrert";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        IContent attendeeContent = _contentService.GetById(int.Parse(model.AttendeeId))!;
        attendeeContent.SetValue("shownUp", model.ShownUp);
        PublishResult result = _contentService.SaveAndPublish(attendeeContent);
        if (!result.Success)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
        }

        TempData["status"] = "Registrering vellykket";

        if (model.ShownUp.Equals("late", StringComparison.CurrentCultureIgnoreCase))
        {
            IMember member = _memberService.GetByKey(Guid.Parse(model.AttendeeMemberId))!;
            TempData["status"] = "Registrering vellykket. Prikk registrert";
            _pointsService.GivePoints(member, "Du møtte for sent til et arrangement", 1);
        }

        if (model.ShownUp.Equals("no", StringComparison.CurrentCultureIgnoreCase))
        {
            IMember member = _memberService.GetByKey(Guid.Parse(model.AttendeeMemberId))!;
            TempData["status"] = "Registrering vellykket. Prikk registrert";
            _pointsService.GivePoints(member, "Du møtte ikke til et arrangement arrangement", 2);
        }

        return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
    }
}