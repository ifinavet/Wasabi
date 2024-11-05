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

    /// <summary>
    ///     Handles the request to render the start page.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> that renders the start page view.</returns>
    public override IActionResult Index()
    {
        JobListing[] hits = _jobListingSearchService.GetJobListings("").OfType<JobListing>().Where(x => x.IsVisible())
            .OrderBy(x => x.Deadline).Take(3).ToArray();

        // Fetches all events and filters them to only include visible ones
        Event[] events = _eventSearchService.GetAllEvents().OfType<Event>().Where(x => x.IsVisible()).Take(3).ToArray();

        StartPageViewModel startPageViewModel = new(CurrentPage!, _publishedValueFallback)
        {
            JobListings = hits,
            Events = events
        };

        return CurrentTemplate(startPageViewModel);
    }
}