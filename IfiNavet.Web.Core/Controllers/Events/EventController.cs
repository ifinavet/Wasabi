using IfiNavet.Web.Core.Services.JobListings;
using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using IfiNavet.Web.Core.Models.JobListings;
using Umbraco.Cms.Core.Services;

namespace IfiNavet.Web.Core.Controllers.Events;

public class EventController : RenderController
{
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IMemberManager _memberManager;
    private readonly ServiceContext _serviceContext;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IContentService _contentService;

    public EventController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IMemberManager memberManager,
        IJobListingSearchService IJobListingSearchService,
        IVariationContextAccessor variationContextAccessor,
        ServiceContext serviceContext,
        IContentService contentService
    )
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _jobListingSearchService = IJobListingSearchService;
        _memberManager = memberManager;
        _serviceContext = serviceContext;
        _variationContextAccessor = variationContextAccessor;
        _contentService = contentService;
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

        // TODO! Check if current member is attending the event.
        MemberIdentityUser? currentMember = await _memberManager.GetCurrentMemberAsync();

        // Related job to the event
        var companyUdi = _contentService.GetById(model.HostingCompany.Id)!.GetUdi().ToString();
        var relatedJobListing = _jobListingSearchService.GetJobListingsByCompanyUdi(companyUdi);
        
        EventViewModel viewModel = new EventViewModel(CurrentPage!, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
        {
            JobListings = new JobListingsSearchResultModel
            {
                Hits = relatedJobListing.ToList()
            },
            IsRegistrationOpen = isRegistrationOpen,
            AmountOfAttendees = model.Children.Count(),
            IsCurrentMemberAttending = false, // Temp value
            HostingCompany = (Company)model.HostingCompany,
            Organizers = model.Organizer?.Select(static x => x as StudentMember).ToArray(),
            ExternalURL = model.ExternalUrl ?? string.Empty,
        };

        return CurrentTemplate(viewModel);
    }
}