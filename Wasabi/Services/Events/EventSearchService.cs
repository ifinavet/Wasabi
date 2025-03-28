using Examine;
using Examine.Search;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.Services.Events;

public class EventSearchService(
    IExamineManager examineManager,
    UmbracoHelper umbracoHelper)
    : IEventSearchService
{
    /// <summary>
    ///     Retrieves all events from the external index.
    /// </summary>
    /// <returns>
    ///     An <see cref="IEnumerable{IPublishedContent}" /> containing all events if found; otherwise, an empty enumerable.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no index is found with the specified name.
    /// </exception>
    public IEnumerable<IPublishedContent?> GetAllEvents()
    {
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex? index))
            throw new InvalidOperationException(
                $"No index found with name {Constants.UmbracoIndexes.ExternalIndexName}");

        IOrdering? query = index
            .Searcher
            .CreateQuery("content")
            .NodeTypeAlias(Event.ModelTypeAlias)
            .And()
            .RangeQuery<DateTime>(
                ["eventDate"],
                DateTime.Today,
                DateTime.MaxValue,
                true,
                false)
            .OrderBy(new SortableField("eventDate", SortType.Long));

        IEnumerable<string> ids = query.Execute().Select(x => x.Id);

        List<string> enumerable = ids.ToList();
        if (enumerable.IsNullOrEmpty()) yield break;
        foreach (string id in enumerable)
            yield return umbracoHelper.Content(id);
    }

    public IEnumerable<IPublishedContent?> GetAllEventOfCurrentPeriod()
    {
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex? index))
            throw new InvalidOperationException(
                $"No index found with name {Constants.UmbracoIndexes.ExternalIndexName}");

        IOrdering? query = index
            .Searcher
            .CreateQuery("content")
            .NodeTypeAlias(Event.ModelTypeAlias)
            .And()
            .RangeQuery<DateTime>(
                ["eventDate"],
                DateTime.Today.Month <= 6
                    ? new DateTime(DateTime.Today.Year, 1, 1) // First half (Jan-Jun)
                    : new DateTime(DateTime.Today.Year, 7, 1), // Second half (Jul-Dec)
                DateTime.Today.Month <= 6
                    ? new DateTime(DateTime.Today.Year, 6, 30) // End of first half
                    : new DateTime(DateTime.Today.Year, 12, 31), // End of second half
                true,
                true)
            .OrderBy(new SortableField("eventDate", SortType.Long)); 

        IEnumerable<string> ids = query.Execute().Select(x => x.Id);

        List<string> enumerable = ids.ToList();
        if (enumerable.IsNullOrEmpty()) yield break;
        foreach (string id in enumerable)
            yield return umbracoHelper.Content(id);
    }
}