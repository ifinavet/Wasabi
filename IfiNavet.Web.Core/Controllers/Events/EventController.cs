using IfiNavet.Web.Core.Services.JobListings;
using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using IfiNavet.Web.Core.Models;

namespace IfiNavet.Web.Core.Controllers.Events;

public class EventController : RenderController
{
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IMemberManager _memberManager;

    public EventController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IMemberManager memberManager
    )
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _memberManager = memberManager;
    }

    [NonAction]
    public sealed override IActionResult Index() => throw new NotImplementedException();
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Event model = new Event(CurrentPage!, _publishedValueFallback);

        // Auto open registration, adjusted for timezone
        TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTime dateTimeCET = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cetZone);
        bool isRegistrationOpen = model.RegistrationDate > dateTimeCET;

        MemberIdentityUser? currentMember = await _memberManager.GetCurrentMemberAsync();
        
        EventViewModel viewModel = new EventViewModel()
        {
            JobListings = null,
            IsRegistrationOpen = isRegistrationOpen,
            AmountOfAttendees = model.Children.Count(),
            IsCurrentMemberAttending = false, // Temp value
            HostingCompany = model.HostingCompany as Company,
            Organizers = model.Organizer?.Select(x => x as StudentMember).ToArray(),
            ExternalURL = model.ExternalUrl ?? string.Empty,
        };

        return CurrentTemplate(CurrentPage);
    }
}