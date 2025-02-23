using Umbraco.Cms.Core.Composing;
using Wasabi.Services.Events;
using Wasabi.Services.JobListings;

namespace Wasabi.Services;

public class RegisterSiteServiceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Adding the job filter and search
        builder.Services.AddTransient<IJobListingSearchService, JobListingSearchService>();
        // Adding events filter and search
        builder.Services.AddTransient<IEventSearchService, EventSearchService>();
        // Adding service to send emails for verification and password reset, and more.
        builder.Services.AddTransient<IEmailService, EmailService>();
        // Adding captcha validation service
        builder.Services.AddTransient<IGoogleReCaptchaService, GoogleReCaptchaService>();
        // Adding service to get company images and members image.
        builder.Services.AddTransient<IImageService, ImageService>();
        // Adding service to manage points.
        builder.Services.AddTransient<IPointsService, PointsService>();
    }
}