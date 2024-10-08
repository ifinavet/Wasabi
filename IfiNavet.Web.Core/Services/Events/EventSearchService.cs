using Examine;
using Examine.Search;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Services.Events;

public class EventSearchService(
    IExamineManager examineManager,
    UmbracoHelper umbracoHelper)
    : IEventSearchService
{
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
                minInclusive: true,
                maxInclusive: false)
            .OrderBy(new SortableField("eventDate", SortType.Long));

        IEnumerable<string> ids = query.Execute().Select(x => x.Id);

        List<string> enumerable = ids.ToList();
        if (enumerable.IsNullOrEmpty()) yield break;
        foreach (string id in enumerable)
            yield return umbracoHelper.Content(id);
    }
}