using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.ViewModels;

public class StartPageViewModel : PublishedContentWrapped
{
    public StartPageViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public StartPage StartPageModel { get; set; }
    public Company? Partner { get; set; }

    public JobListing[] JobListings { get; set; }

    public Event[] Events { get; set; }
}