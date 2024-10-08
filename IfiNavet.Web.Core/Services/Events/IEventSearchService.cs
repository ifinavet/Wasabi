using Umbraco.Cms.Core.Models.PublishedContent;

namespace IfiNavet.Web.Core.Services.Events;

public interface IEventSearchService
{
    IEnumerable<IPublishedContent?> GetAllEvents();
}