using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Services.JobListings;
using Wasabi.ViewModels.JobListings;

namespace Wasabi.Controllers.JobListings;

public class JobListingsController : RenderController
{
    private readonly IJobListingSearchService _jobListingSearchService;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public JobListingsController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IJobListingSearchService jobListingSearchService,
        IPublishedValueFallback publishedValueFallback)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _jobListingSearchService = jobListingSearchService;
        _publishedValueFallback = publishedValueFallback;
    }

    /// <summary>
    ///     GET: Job listings
    ///     Hijacks route to fetch job listings and filter them based on search parameters and job type
    /// </summary>
    /// <param name="query">Search query for job listing</param>
    /// <param name="jobType">Job listings types</param>
    [HttpGet]
    public IActionResult Index([FromQuery(Name = "query")] string query, [FromQuery(Name = "jobType")] string jobType)
    {
        // Fetches job listings based on query. Returns all if query is empty
        IEnumerable<IPublishedContent> hits = _jobListingSearchService.GetJobListings(query);
        List<JobListing> filteredSearchResult = hits.OfType<JobListing>().ToList();

        JobListingsViewModel viewModel = new(CurrentPage!, _publishedValueFallback)
        {
            QueryString = query,
            JobTypes = GetJobTypeFilters(filteredSearchResult, jobType),
            IsFilteredByJobType = !string.IsNullOrWhiteSpace(jobType)
        };

        // Filter by job type, if type is specified
        if (!string.IsNullOrWhiteSpace(jobType))
            filteredSearchResult = filteredSearchResult
                .Where(x => x.JobType!.Equals(jobType, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

        viewModel.SearchResult = filteredSearchResult;

        return CurrentTemplate(viewModel);
    }

    /// <summary>
    ///     Filters job listings by job types
    /// </summary>
    /// <param name="jobListings"></param>
    /// <param name="activeJobType"></param>
    /// <returns>Enumerable of job listings limited to the specified job type</returns>
    private static IEnumerable<JobTypesFilter> GetJobTypeFilters(IEnumerable<JobListing> jobListings,
        string activeJobType)
    {
        IEnumerable<JobTypesFilter> jobTypes = jobListings
            .GroupBy(x => x.JobType)
            .Select(g => g.Key)
            .Select(x => new JobTypesFilter
            {
                JobTypeAlias = x!,
                IsActive = x!.Equals(activeJobType, StringComparison.InvariantCultureIgnoreCase)
            });

        return jobTypes;
    }
}