using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using PostHog;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Models.JobListings;
using Wasabi.Services.JobListings;
using Wasabi.ViewModels.JobListings;

namespace Wasabi.Controllers.JobListings;

public class JobListingController : RenderController
{
    private readonly IContentService _contentService;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IPostHogClient _postHogClient;

    public JobListingController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IContentService contentService,
        IJobListingSearchService jobListingSearchService,
        IPostHogClient postHogClient) : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _contentService = contentService;
        _jobListingSearchService = jobListingSearchService;
        _postHogClient = postHogClient;
    }

    /// <summary>
    ///     Overrides the default index action to fetch other job listings from the same company.
    /// </summary>
    /// <returns>
    ///     An <see cref="IActionResult" /> that renders the view with the job listing and related job listings.
    /// </returns>
    public override IActionResult Index()
    {
        JobListing jobListing = new(CurrentPage!, _publishedValueFallback);

        string companyUdi = _contentService.GetById(jobListing.Employer!.Id)!.GetUdi().ToString();

        // Finds job listings by same company
        List<IPublishedContent>? relatedJobListings;
        if (companyUdi.IsNullOrWhiteSpace())
            relatedJobListings = null;
        else
            relatedJobListings = _jobListingSearchService
                .GetJobListingsByCompanyUdi(companyUdi)
                .Where(x => x.Id != jobListing.Id)
                .ToList();


        JobListingViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            JobListings = new JobListingsSearchResultModel
            {
                Hits = relatedJobListings
            }
        };

        return CurrentTemplate(viewModel);
    }
}