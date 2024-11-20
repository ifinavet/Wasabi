using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Wasabi.Controllers.Events;

public class EventsController : RenderController
{
    private readonly IPublishedValueFallback _publishedValueFallback;

    public EventsController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback
    )
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
    }

    /// <summary>
    ///     Redirects to current semester
    /// </summary>
    public override IActionResult Index()
    {
        Umbraco.Cms.Web.Common.PublishedModels.Events eventsModel = new(CurrentPage!, _publishedValueFallback);
        return Redirect(eventsModel.ActiveSemester!.Url());
    }
}