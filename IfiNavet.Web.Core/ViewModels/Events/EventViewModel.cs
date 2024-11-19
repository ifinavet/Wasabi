using IfiNavet.Web.Core.Models.JobListings;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.ViewModels.Events;

public class EventViewModel : Event
{
    public EventViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public required JobListingsSearchResultModel JobListings { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public int AmountOfAttendees { get; set; }
    public bool IsCurrentMemberAttending { get; set; }
    public required StudentMember?[] Organizers { get; set; }
}