using Umbraco.Cms.Core.Models.PublishedContent;

namespace Wasabi.Services.Events;

public interface IEventSearchService
{
    IEnumerable<IPublishedContent?> GetAllEvents();
}