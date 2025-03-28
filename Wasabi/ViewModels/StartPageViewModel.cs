using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.ViewModels;

public class StartPageViewModel : StartPage
{
    public StartPageViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public JobListing[]? JobListings { get; set; }

    public Event[]? Events { get; set; }
}