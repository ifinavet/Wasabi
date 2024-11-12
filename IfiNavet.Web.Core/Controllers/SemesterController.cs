using IfiNavet.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Controllers;

public class SemesterController : RenderController
{
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly UmbracoHelper _umbracoHelper;

    public SemesterController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        UmbracoHelper umbracoHelper)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _umbracoHelper = umbracoHelper;
    }

    /// <summary>
    ///     Gets the events for the semester, sorts them and groups them by month
    /// </summary>
    /// <returns>Viewmodel with all the events for the semeste</returns>
    public override IActionResult Index()
    {
        IGrouping<string, Event>[]? monthGroups = _umbracoHelper
            .Content(CurrentPage!.Id)?.Children.OfType<Event>()
            .OrderBy(e => e.EventDate)
            .GroupBy(e => e.EventDate.ToString("MM"))
            .OrderBy(e => e.Key)
            .ToArray();

        SemesterViewModel semesterViewModel = new(CurrentPage!, _publishedValueFallback)
        {
            MonthGroups = monthGroups
        };

        return CurrentTemplate(semesterViewModel);
    }
}