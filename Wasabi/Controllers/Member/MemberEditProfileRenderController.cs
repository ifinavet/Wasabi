using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Wasabi.Services;
using Wasabi.ViewModels.Member;

namespace Wasabi.Controllers.Member;

public class MemberEditProfileRenderController : RenderController
{
    private readonly ILogger<RenderController> _logger;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IPointsService _pointsService;
    private readonly IPublishedValueFallback _publishedValueFallback;

    public MemberEditProfileRenderController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberManager memberManager,
        IMemberService memberService,
        IPublishedValueFallback publishedValueFallback,
        IPointsService pointsService)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _publishedValueFallback = publishedValueFallback;
        _logger = logger;
        _pointsService = pointsService;
    }

    /// <summary>
    ///     Renders the profile edit page for the current member.
    /// </summary>
    /// <returns>
    ///     An <see cref="IActionResult" /> that renders the profile edit page view.
    /// </returns>
    [Route("/profil/endre-profil/")]
    public override IActionResult Index()
    {
        MemberIdentityUser? currentMember = _memberManager.GetCurrentMemberAsync().Result;

        MemberEditProfileFormViewModel formViewModel = new();

        MemberEditProfileViewModel model = new(CurrentPage!, _publishedValueFallback);

        if (currentMember == null) return CurrentTemplate(model);

        IMember currentStudent = _memberService.GetByKey(currentMember.Key)!;
        formViewModel.FirstName = currentStudent.GetValue<string>("firstName") ?? string.Empty;
        formViewModel.LastName = currentStudent.GetValue<string>("lastName") ?? string.Empty;
        formViewModel.PreferredLanguage = currentStudent.GetValue<string>("preferredLanguage") ?? "norwegian";
        formViewModel.Semester = currentStudent.GetValue<int>("semester");
        formViewModel.StudyProgram = currentStudent.GetValue<string>("studieprogram") ?? "ingen";

        model.FormViewModel = formViewModel;
        model.MemberPoints = _pointsService.ParsedPointEntries(currentStudent.GetValue<string>("points"));
        _logger.LogDebug("Member points ${1}", model.MemberPoints); 
        
        _logger.LogDebug("Member edit profile rendered");

        return CurrentTemplate(model);
    }
}