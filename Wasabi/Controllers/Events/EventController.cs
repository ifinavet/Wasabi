using System.Drawing;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using NUglify.Helpers;
using QRCoder;
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
    private readonly ILogger<RenderController> _logger;

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
        _logger = logger;
    }

    /// <summary>
    ///     Handles the event attendee registration process,
    ///     and the information about the students registered and attending the event
    /// </summary>
    /// <param name="columns">A set of columns to be included in the registration view.</param>
    /// <returns>An IActionResult containing the event attendee registration view.</returns>
    [HttpGet]
    [UmbracoMemberAuthorize("StudentMember", "NavetEventAdmins", "")]
    public IActionResult EventAttendeeRegistration([FromQuery(Name = "columns")] HashSet<string> columns)
    {
        Event model = new(CurrentPage, _publishedValueFallback);

        List<AdministrationAttendee> attendees = new();
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
                    PreferredLanguage = member.PreferredLanguage,
                    AttendeeMemberId = member.Key.ToString(),
                    AttendeeId = attendee.Id.ToString(),
                    AttendeeStatus = attendee.ShownUp ?? "Ikke registrert"
                });
        }

        EventAttendeeRegistrationViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            EventId = model.Id,
            Attendees = attendees,
            StudyProgramAndSemester =
                StudyProgramAndSemester((model.Children<Attendee>() ?? Array.Empty<Attendee>()).ToList())
        };

        foreach (KeyValuePair<string, bool> column in viewModel.Columns)
            viewModel.Columns[column.Key] = columns.Contains(column.Key);

        return CurrentTemplate(viewModel);
    }

    /// <summary>
    ///     Processes a list of attendees to group them by their study program and semester.
    /// </summary>
    /// <param name="attendees">A list of attendees to process.</param>
    /// <returns>
    ///     A dictionary where the key is the study program and the value is a sorted dictionary
    ///     where the key is the semester and the value is the count of attendees in that semester.
    /// </returns>
    private static Dictionary<string, SortedDictionary<int, int>> StudyProgramAndSemester(List<Attendee> attendees)
    {
        Dictionary<string, SortedDictionary<int, int>> studyProgramAndSemester = new();

        foreach (Attendee attendee in attendees)
        {
            if (attendee.AttendingMember is not StudentMember member) continue;

            string studyProgram = member.Studieprogram is not ("" or null) ? member.Studieprogram : "NA";
            int semester = member.Semester;

            if (!studyProgramAndSemester.ContainsKey(studyProgram))
                studyProgramAndSemester[studyProgram] = new SortedDictionary<int, int>();

            if (!studyProgramAndSemester[studyProgram].ContainsKey(semester))
                studyProgramAndSemester[studyProgram].Add(semester, 0);

            studyProgramAndSemester[studyProgram][semester] += 1;
        }

        return studyProgramAndSemester;
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
            try
            {
                Console.WriteLine("Updated started");
                foreach (Attendee attendee in model.Children<Attendee>()?.Where(a => true) ?? [])
                {
                    if (attendee.MemberId != string.Empty)
                        continue;

                    IContent a = _contentService.GetById(attendee.Id)!;
                    a.SetValue("memberId", attendee.AttendingMember!.Key);
                    _contentService.Save(a);
                }

                Console.WriteLine("Update done");
            }
            catch (Umbraco.Cms.Core.Exceptions.PanicException ex) when (ex.Message.Contains("failed to get content"))
            {
                _logger.LogWarning("Content missing during migration: {message}", ex.Message);
            }
        }

        // TODO! OK for now, future figure out how to improve the speed.
        // Checks if the current member is registered to the event
        MemberIdentityUser? currentMember = await _memberManager.GetCurrentMemberAsync();
        bool? isCurrentMemberAttending = false;
        Attendee? currentMemberAttendee = null;

        if (currentMember != null && isRegistrationOpen)
        {
            try
            {
                _logger.LogDebug("Member key " + currentMember.Key + " Member id " + currentMember.Id);

                currentMemberAttendee =
                    (model.Children<Attendee>() ?? []).Where(a => true).FirstOrDefault(a =>
                        a.MemberId == currentMember.Key.ToString());
                isCurrentMemberAttending = currentMemberAttendee != null;
            }
            catch (Umbraco.Cms.Core.Exceptions.PanicException ex) when (ex.Message.Contains("failed to get content"))
            {
                _logger.LogWarning("Content missing when checking member registration: {message}", ex.Message);
            }
        }

        // Related jobs to the event
        string companyUdi = _contentService.GetById(model.HostingCompany!.Id)!.GetUdi().ToString();
        IEnumerable<IPublishedContent> relatedJobListing =
            _jobListingSearchService.GetJobListingsByCompanyUdi(companyUdi);

        // CurrentMembers qrCode
        string? qrCode = null;
        if (currentMemberAttendee != null && (model.EventDate - dateTimeCet).TotalHours < 12)
            qrCode = CurrentMemberIdentityQrCode(currentMemberAttendee, model.Id);

        int attendeeCount = 0;
        try
        {
            attendeeCount = model.Children<Attendee>()?.Count() ?? 0;
        }
        catch (Umbraco.Cms.Core.Exceptions.PanicException ex) when (ex.Message.Contains("failed to get content"))
        {
            _logger.LogWarning("Content missing when counting attendees: {message}", ex.Message);
        }

        EventViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            JobListings = new JobListingsSearchResultModel
            {
                Hits = relatedJobListing.ToList()
            },
            IsRegistrationOpen = isRegistrationOpen,
            AmountOfAttendees = attendeeCount,
            IsCurrentMemberAttending = isCurrentMemberAttending ?? false,
            Organizers = model.Organizer?.Select(static x => x as StudentMember).ToArray()!,
            CurrentAttendeeQrCode = qrCode
        };

        return CurrentTemplate(viewModel);
    }

    private static string CurrentMemberIdentityQrCode(Attendee currentAttendee, int eventId)
    {
        Dictionary<string, string> qrCodeInformation = new()
        {
            { "eventId", eventId.ToString() },
            { "attendeeId", currentAttendee.Id.ToString() },
            { "attendeeMemberId", currentAttendee.MemberId ?? currentAttendee.AttendingMember!.Key.ToString() }
        };

        QRCodeGenerator qrCodeGenerator = new();
        QRCodeData qrCodeData =
            qrCodeGenerator.CreateQrCode(JsonSerializer.Serialize(qrCodeInformation), QRCodeGenerator.ECCLevel.Q);
        Base64QRCode qrCode = new(qrCodeData);
        string qrCodeImageAsBase64 = qrCode.GetGraphic(15, Color.FromArgb(42, 72, 108), Color.White);

        string htmlImgSrc = $"data:image/jpeg;base64,{qrCodeImageAsBase64}";

        return htmlImgSrc;
    }
}