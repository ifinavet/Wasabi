using IfiNavet.Web.Core.Models.JobListings;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.ViewModels;

public class EventViewModel
{
        public JobListingsSearchResultModel JobListings { get; set; }
        public bool IsRegistrationOpen { get; set; }
        public int AmountOfAttendees { get; set; }
        public bool IsCurrentMemberAttending { get; set; }
        public Company HostingCompany { get; set; }
        public StudentMember[] Organizers { get; set; }
        public string ExternalURL { get; set; }

}