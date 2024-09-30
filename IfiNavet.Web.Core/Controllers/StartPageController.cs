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
    private readonly IPublishedValueFallback _publishedValueFallback;

    public StartPageController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
    }
    
    public override IActionResult Index()
    {
        var startPageModel = new StartPage(CurrentPage!, _publishedValueFallback);
        var startPageViewModel = new StartPageViewModel(CurrentPage!, _publishedValueFallback)
        {
            StartPageModel = startPageModel
        };

        return CurrentTemplate(startPageViewModel);
    }
}