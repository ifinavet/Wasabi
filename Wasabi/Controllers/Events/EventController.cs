using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Models.JobListings;
using Wasabi.Services.JobListings;
using Wasabi.ViewModels.Events;

namespace Wasabi.Controllers.Events;

public class EventController : RenderController
{
    private readonly IContentService _contentService;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IMemberManager _memberManager;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public EventController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IMemberManager memberManager,
        IJobListingSearchService jobListingSearchService,
        IContentService contentService
    )
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _jobListingSearchService = jobListingSearchService;
        _memberManager = memberManager;
        _contentService = contentService;
    }

    [HttpGet]
    [UmbracoMemberAuthorize("StudentMember", "NavetEventAdmins", "")]
    public IActionResult EventAttendeeRegistration([FromQuery(Name = "columns")] HashSet<string> columns)
    {
        Event model = new(CurrentPage, _publishedValueFallback);

        List<AdministrationAttendee> attendees = [];
        foreach (Attendee attendee in model.Children<Attendee>()!)
        {
            StudentMember? member = (StudentMember?)attendee.AttendingMember;
            if (member is not null)
                attendees.Add(new AdministrationAttendee
                {
                    FullName = member.FirstName + " " + member.LastName,
                    Username = attendee.AttendingMember!.GetProperty("Username")!.GetValue()!.ToString()!,
                    Allergies = attendee.Allergies,
                    Email = attendee.AttendingMember!.GetProperty("Email")!.GetValue()!.ToString()!,
                    PreferredLanguage = member.PreferredLanguage
                });
        }

        EventAttendeeRegistrationViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            EventId = model.Id,
            Attendees = attendees
        };

        foreach (KeyValuePair<string, bool> column in viewModel.Columns)
            viewModel.Columns[column.Key] = columns.Contains(column.Key);


        return CurrentTemplate(viewModel);
    }

    /// <summary>
    ///     Handles the event page.
    ///     Making sure the event opens on time
    ///     Checking if current member is registered
    ///     Getting related job listings to the event.
    ///     Getting all the information about the event and organizer
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="migrate">Set true if we want to migrate the attendees</param>
    /// <returns>A viewmodel with the information for the event</returns>
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery(Name = "migrate")] bool migrate,
        CancellationToken cancellationToken)
    {
        Event model = new(CurrentPage!, _publishedValueFallback);

        // Auto open registration, adjusted for timezone
        TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTime dateTimeCet = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cetZone);
        bool isRegistrationOpen = model.RegistrationDate < dateTimeCet;

        if (migrate)
        {
            Console.WriteLine("Updated started");
            foreach (Attendee attendee in model
                         .Children<Attendee>()!)
            {
                if (attendee.MemberId != string.Empty)
                    continue;

                IContent a = _contentService.GetById(attendee.Id)!;
                a.SetValue("memberId", attendee.AttendingMember!.Key);
                _contentService.Save(a);
            }

            Console.WriteLine("Update done");
        }

        // TODO! OK for now, future figure out how to improve the speed.
        // Checks if the current member is registered to the event
        MemberIdentityUser? currentMember = await _memberManager.GetCurrentMemberAsync();
        bool? isCurrentMemberAttending = false;
        if (currentMember != null && isRegistrationOpen)
            isCurrentMemberAttending = model.Children<Attendee>()?
                .Any(a => a.MemberId == currentMember.Key.ToString());

        // Related job to the event
        string companyUdi = _contentService.GetById(model.HostingCompany!.Id)!.GetUdi().ToString();
        IEnumerable<IPublishedContent> relatedJobListing =
            _jobListingSearchService.GetJobListingsByCompanyUdi(companyUdi);

        EventViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            JobListings = new JobListingsSearchResultModel
            {
                Hits = relatedJobListing.ToList()
            },
            IsRegistrationOpen = isRegistrationOpen,
            AmountOfAttendees = model.Children.Count(),
            IsCurrentMemberAttending = isCurrentMemberAttending ?? false,
            Organizers = model.Organizer?.Select(static x => x as StudentMember).ToArray()!
        };

        return CurrentTemplate(viewModel);
    }
}