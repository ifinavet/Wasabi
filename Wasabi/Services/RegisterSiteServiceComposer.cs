using Umbraco.Cms.Core.Composing;
using Wasabi.Services.Events;
using Wasabi.Services.JobListings;

namespace Wasabi.Services;

public class RegisterSiteServiceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IJobListingSearchService, JobListingSearchService>(); // Adding the job filter and search
        builder.Services.AddTransient<IEventSearchService, EventSearchService>(); // Adding events filter and search
        builder.Services.AddTransient<IEmailService, EmailService>();
    }
}