using Umbraco.Cms.Core.Models.PublishedContent;

namespace IfiNavet.Web.Core.Models.JobListings;

public class JobListingsSearchResultModel
{
    public List<IPublishedContent>? Hits { get; set; }
}