using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Models.JobListings;

namespace Wasabi.ViewModels.JobListings;

public class JobListingViewModel : JobListing
{
    public JobListingViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public required JobListingsSearchResultModel JobListings { get; set; }
}