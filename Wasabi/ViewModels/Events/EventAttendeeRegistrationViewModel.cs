using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.ViewModels.Events;

public class EventAttendeeRegistrationViewModel : Event
{
    public EventAttendeeRegistrationViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public required int EventId { get; set; }
    public required List<AdministrationAttendee> Attendees { get; init; }
}

public class AdministrationAttendee
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? Allergies { get; set; }
    public string? PreferredLanguage { get; set; }
}