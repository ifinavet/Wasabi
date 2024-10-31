using IfiNavet.Web.Core.Models.JobListings;
using IfiNavet.Web.Core.Services.JobListings;
using IfiNavet.Web.Core.ViewModels.JobListings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Controllers.JobListings;

public class JobListingController : RenderController
{
    private readonly IContentService _contentService;
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public JobListingController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IContentService contentService,
        IJobListingSearchService jobListingSearchService
    ) : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _publishedValueFallback = publishedValueFallback;
        _contentService = contentService;
        _jobListingSearchService = jobListingSearchService;
    }

    /// <summary>
    ///     Overrides default index to fetch other job listings form same company
    /// </summary>
    public override IActionResult Index()
    {
        JobListing jobListing = new(CurrentPage!, _publishedValueFallback);

        string companyUdi = _contentService.GetById(jobListing.Employer.Id).GetUdi().ToString();

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
            JobListing = jobListing,
            Company = jobListing.Employer as Company,
            JobListings = new JobListingsSearchResultModel
            {
                Hits = relatedJobListings
            }
        };

        return CurrentTemplate(viewModel);
    }
}