using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.ViewModels.JobListings;

public class JobListingsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
    : PublishedContentWrapped(content, publishedValueFallback)
{
    public string QueryString { get; set; } = string.Empty;

    public IEnumerable<JobListing>? SearchResult { get; set; }

    public IEnumerable<JobTypesFilter>? JobTypes { get; set; }

    public bool IsFilteredByJobType { get; set; }
}

public class JobTypesFilter
{
    public string JobTypeAlias { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}