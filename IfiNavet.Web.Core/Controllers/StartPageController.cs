using IfiNavet.Web.Core.Services.Events;
using IfiNavet.Web.Core.Services.JobListings;
using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Controllers;

public class StartPageController : RenderController
{
    private readonly IEventSearchService _eventSearchService;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public StartPageController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IJobListingSearchService jobListingSearchService,
        IEventSearchService eventSearchService)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _jobListingSearchService = jobListingSearchService;
        _eventSearchService = eventSearchService;
    }

    public override IActionResult Index()
    {
        StartPage startPageModel = new(CurrentPage!, _publishedValueFallback);

        // Fetches job listings based on query. Returns all if query is empty
        JobListing[] hits = _jobListingSearchService.GetJobListings("").OfType<JobListing>().Where(x => x.IsVisible())
            .OrderBy(x => x.Deadline).ToArray();

        Event[] events = _eventSearchService.GetAllEvents().OfType<Event>().Where(x => x.IsVisible()).ToArray();

        StartPageViewModel startPageViewModel = new(CurrentPage!, _publishedValueFallback)
        {
            StartPageModel = startPageModel,
            Partner = (Company)startPageModel.Partner,
            JobListings = hits,
            Events = events
        };

        return CurrentTemplate(startPageViewModel);
    }
}