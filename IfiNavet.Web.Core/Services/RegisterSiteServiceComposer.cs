using IfiNavet.Web.Core.Services.Events;
using IfiNavet.Web.Core.Services.JobListings;
using Umbraco.Cms.Core.Composing;

namespace IfiNavet.Web.Core.Services;

public class RegisterSiteServiceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IJobListingSearchService, JobListingSearchService>(); // Adding the job filter and search
        builder.Services.AddTransient<IEventSearchService, EventSearchService>(); // Adding events filter and search
        builder.Services.AddTransient<IEmailService, EmailService>();
    }
}