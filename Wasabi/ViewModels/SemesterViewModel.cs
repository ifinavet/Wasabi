using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.ViewModels;

public class SemesterViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
    : PublishedContentWrapped(content, publishedValueFallback)
{
    public required string ActiveMonth { get; set; }
    public IGrouping<string, Event>[]? MonthGroups { get; set; }
}