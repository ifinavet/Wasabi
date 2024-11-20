using Umbraco.Cms.Core.Models.PublishedContent;

namespace Wasabi.Models.JobListings;

public class JobListingsSearchResultModel
{
    public List<IPublishedContent>? Hits { get; set; }
}