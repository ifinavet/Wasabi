using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Services.Events;
using Wasabi.ViewModels;

namespace Wasabi.Controllers;

public class SemesterController : RenderController
{
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly UmbracoHelper _umbracoHelper;
    private readonly IEventSearchService _eventSearchService;

    public SemesterController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        UmbracoHelper umbracoHelper,
        IEventSearchService eventSearchService)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _umbracoHelper = umbracoHelper;
        _eventSearchService = eventSearchService;
    }

    /// <summary>
    ///     Gets the events for the semester, sorts them and groups them by month
    /// </summary>
    /// <returns>Viewmodel with all the events for the semester</returns>
    [HttpGet]
    public IActionResult Index([FromQuery(Name = "month")] string month)
    {
        IGrouping<string, Event>[] eventsByMonth = _eventSearchService
            .GetAllEventOfCurrentPeriod()
            .OfType<Event>()
            .Where(a => a.IsVisible())
            .OrderBy(e => e.EventDate)
            .GroupBy(e => e.EventDate.ToString("MM"))
            .OrderBy(e => e.Key)
            .ToArray();

        SemesterViewModel semesterViewModel = new(CurrentPage!, _publishedValueFallback)
        {
            MonthGroups = eventsByMonth,
            ActiveMonth = month.IsNullOrWhiteSpace() ? DateTime.Today.ToString("MM") : month
        };

        return CurrentTemplate(semesterViewModel);
    }
}