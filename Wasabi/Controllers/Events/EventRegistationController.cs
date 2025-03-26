using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PostHog;
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
using Wasabi.Models;
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
    private readonly IPostHogClient _postHogClient;

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
        IPointsService pointsService,
        IPostHogClient postHogClient)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManger = memberManger;
        _memberService = memberService;
        _contentService = contentService;
        _logger = logger;
        _pointsService = pointsService;
        _postHogClient = postHogClient;
    }

    [HttpPost]
    public async Task<IActionResult> Register(EventRegistrationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            TempData["error"] = true;
            _logger.LogInformation("Event registration failed. \n\t ModelState is not valid");
            return CurrentUmbracoPage();
        }

        MemberIdentityUser? currentMember = await _memberManger.GetCurrentMemberAsync();
        IMember currentStudent = _memberService.GetByKey(currentMember!.Key)!;
        if (!Studieprogram.StudieprogramList.Contains(currentStudent.GetValue<string>("studieprogram") ?? string.Empty))
        {
            TempData["status"] = "Du har ikke valgt et gyldig studieprogram";
            TempData["error"] = true;
            
            _logger.LogInformation("Event registration failed. \n\t Study program is not valid");
            return CurrentUmbracoPage();
        }

        if (currentStudent.GetValue<int>("semester") < 1)
        {
            TempData["status"] = "Ugyldig semester verdi";
            TempData["error"] = true;
            
            _logger.LogInformation("Event registration failed. \n\t Semester is not valid");
            return CurrentUmbracoPage();
        }

        List<Point>? pointEntries = _pointsService.ParsedPointEntries(currentStudent.GetValue<string>("points"));
        int? numOfPoints = pointEntries?.Select(entry => entry.Severity).Sum();
        if (numOfPoints >= 3)
        {
            _logger.LogWarning("Member have to many points");
            TempData["status"] = "Du har for mange prikker til å kunne melde deg på dette arrangementet";
            TempData["error"] = true;
            
            _logger.LogInformation("Event registration failed. \n\t To many points");
            return CurrentUmbracoPage();
        }

        if (CurrentPage is not Event || CurrentPage == null)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            TempData["error"] = true;
            
            _logger.LogInformation("Event registration failed. \n\t CurrentPage is not Event or is null");
            return CurrentUmbracoPage();
        }

        Event currentPage = (Event)CurrentPage;

        if (currentPage.Children<Attendee>()!.Any(a => a.MemberId == currentMember.Key.ToString()))
        {
            TempData["status"] = "Du er allerede registert";
            
            _logger.LogInformation("Event registration failed. \n\t Attendee is already registered");
            return CurrentUmbracoPage();
        }

        if (currentPage.ParticipantLimit <= currentPage.Children<Attendee>()!.Count())
        {
            TempData["status"] = "Arrangementet er fullt";
            TempData["error"] = true;
            
            _logger.LogInformation("Event registration failed. \n\t Event is full");
            return CurrentUmbracoPage();
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
            
            _logger.LogInformation("Event registration failed. \n\t Publishing of attendee failed");
            return CurrentUmbracoPage();
        }

        _postHogClient.Capture(currentMember.Key.ToString(), "registered_to_event", properties: new()
        {
            ["event_id"] = currentPage.Key.ToString(),
            ["event_name"] = currentMember.Name!,
        });

        _logger.LogInformation("Event registration succeeded");
        return RedirectToCurrentUmbracoPage();
    }

    public IActionResult Unregister()
    {
        if (CurrentPage is not Event currentPage)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            
            _logger.LogInformation("Event un-registration failed. \n\t CurrentPage is not Event");
            return CurrentUmbracoPage();
        }

        MemberIdentityUser? currentMemberIdentity = _memberManger.GetCurrentMemberAsync().Result;
        if (currentMemberIdentity == null)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            
            _logger.LogInformation("Event un-registration failed. \n\t Member not found");
            return CurrentUmbracoPage();
        }

        int attendeeId = currentPage.Children<Attendee>()!
            .FirstOrDefault(a => a.MemberId == currentMemberIdentity.Key.ToString())!.Id;

        IContent content = _contentService.GetById(attendeeId)!;
        OperationResult result = _contentService.Delete(content);
        if (!result.Success)
        {
            TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
            
            _logger.LogInformation("Event registration failed. \n\t Failed to delete attendee");
            return CurrentUmbracoPage();
        }

        Event? currentEvent = (Event)CurrentPage;
        TimeSpan timeRemaining = currentEvent.EventDate - DateTime.Now;
        _logger.LogDebug("Current member ${currentMember}", currentMemberIdentity.Id);

        if (timeRemaining.TotalHours < 24)
        {
            IMember currentMember = _memberService.GetByKey(currentMemberIdentity.Key)!;
            _pointsService.GivePoints(currentMember, "Du meldte deg av for sent", 1);
            
            _logger.LogWarning("Member given point for late unregistration");
        }

        _logger.LogInformation("Event un-registration succeeded");
        return RedirectToCurrentUmbracoPage();
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

        Attendee? attendee = currentPage.Children<Attendee>()!.FirstOrDefault(a => model.AttendeeId != null && a.Id == int.Parse(model.AttendeeId));
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

        if (model.AttendeeId != null)
        {
            IContent attendeeContent = _contentService.GetById(int.Parse(model.AttendeeId))!;
            attendeeContent.SetValue("shownUp", model.ShownUp);
            PublishResult result = _contentService.SaveAndPublish(attendeeContent);
            if (!result.Success)
            {
                TempData["status"] = "Det har oppstått en feil, prøv igjen senere.";
                return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
            }
        }

        TempData["status"] = "Registrering vellykket";

        if (model.ShownUp != null && model.ShownUp.Equals("late", StringComparison.CurrentCultureIgnoreCase))
        {
            if (model.AttendeeMemberId != null)
            {
                IMember member = _memberService.GetByKey(Guid.Parse(model.AttendeeMemberId))!;
                TempData["status"] = "Registrering vellykket. Prikk registrert";
                _pointsService.GivePoints(member, "Du møtte for sent til et arrangement", 1);
            }
        }

        if (model.ShownUp != null && model.ShownUp.Equals("no", StringComparison.CurrentCultureIgnoreCase))
        {
            if (model.AttendeeMemberId != null)
            {
                IMember member = _memberService.GetByKey(Guid.Parse(model.AttendeeMemberId))!;
                TempData["status"] = "Registrering vellykket. Prikk registrert";
                _pointsService.GivePoints(member, "Du møtte ikke til et arrangement arrangement", 2);
            }
        }

        return RedirectToCurrentUmbracoPage(new QueryString("?altTemplate=EventAttendeeRegistration"));
    }
}