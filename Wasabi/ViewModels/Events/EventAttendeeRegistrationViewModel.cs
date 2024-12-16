using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.ViewModels.Events;

public class EventAttendeeRegistrationViewModel
{
    private string evnetId { get; set; } 
    private Attendee[] attendies { get; set; }
}