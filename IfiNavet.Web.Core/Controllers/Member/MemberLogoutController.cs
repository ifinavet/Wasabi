using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Security;

namespace IfiNavet.Web.Core.Controllers.Member;

public class LogoutController : RenderController
{
    private readonly IMemberSignInManager _memberSignInManager;

    public LogoutController(
        ILogger<LogoutController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberSignInManager memberSignInManager)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _memberSignInManager = memberSignInManager;
    }

    /// <summary>
    ///     Hijacks the route and signs out the user
    /// </summary>
    /// <exception cref="NotImplementedException">Should never be used async index() will always take priority</exception>
    /// <returns>Root page</returns>
    [NonAction]
    public sealed override IActionResult Index()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        await _memberSignInManager.SignOutAsync();
        return Redirect("/");
    }
}