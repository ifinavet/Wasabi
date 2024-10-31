using Examine;
using Examine.Search;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Services.JobListings;

public class JobListingSearchService(
    IExamineManager examineManager,
    UmbracoHelper umbracoHelper)
    : IJobListingSearchService
{
    /// <summary>
    ///     Fetching job listings based on query.
    ///     If query is empty, returns all published job listings
    /// </summary>
    /// <param name="queryString">Search query</param>
    /// <returns>IEnumerable of the job listings within the specified parameters</returns>
    /// <exception cref="InvalidOperationException">Throws error if External Index is not found</exception>
    public IEnumerable<IPublishedContent?> GetJobListings(string queryString)
    {
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex? index))
            throw new InvalidOperationException(
                $"No index found with name {Constants.UmbracoIndexes.ExternalIndexName}");

        IBooleanOperation? query = index
            .Searcher
            .CreateQuery("content")
            .NodeTypeAlias(JobListing.ModelTypeAlias);

        if (!string.IsNullOrEmpty(queryString))
            query = query.And().ManagedQuery(queryString);

        IEnumerable<string> ids = query.Execute().Select(x => x.Id);

        List<string> enumerable = ids.ToList();
        if (enumerable.IsNullOrEmpty()) yield break;
        foreach (string id in enumerable)
            yield return umbracoHelper.Content(id);
    }

    /// <summary>
    ///     Fetches all job listings by same company
    /// </summary>
    /// <param name="companyUdi">Company UDI</param>
    /// <returns>IEnumerable of the job listings</returns>
    /// <exception cref="InvalidOperationException">Throws exception if the ExternalIndex is not found</exception>
    public IEnumerable<IPublishedContent> GetJobListingsByCompanyUdi(string companyUdi)
    {
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex? index))
            throw new InvalidOperationException(
                $"No index found with name {Constants.UmbracoIndexes.ExternalIndexName}");

        IEnumerable<string> hits = index
            .Searcher
            .CreateQuery("content")
            .NodeTypeAlias(JobListing.ModelTypeAlias)
            .And()
            .ManagedQuery(companyUdi)
            .Execute()
            .Select(x => x.Id);

        foreach (string hit in hits) yield return umbracoHelper.Content(hit);
    }
}