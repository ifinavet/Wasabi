using Umbraco.Cms.Core.Models.PublishedContent;

namespace IfiNavet.Web.Core.Services.JobListings;

public interface IJobListingSearchService
{
    IEnumerable<IPublishedContent> GetJobListings(string query);
    IEnumerable<IPublishedContent> GetJobListingsByCompanyUdi(string companyUdi);
}