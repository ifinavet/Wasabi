using Umbraco.Cms.Core.Models.PublishedContent;

namespace Wasabi.Services.JobListings;

public interface IJobListingSearchService
{
    IEnumerable<IPublishedContent> GetJobListings(string query);
    IEnumerable<IPublishedContent> GetJobListingsByCompanyUdi(string companyUdi);
}