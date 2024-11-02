using IfiNavet.Web.Core.Models.JobListings;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.ViewModels.JobListings;

public class JobListingViewModel : JobListing
{
    public JobListingViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public JobListingsSearchResultModel JobListings { get; set; }
    public Company Company { get; set; }
}