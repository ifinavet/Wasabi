using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Wasabi.Models;

namespace Wasabi.ViewModels.Member;

public class MemberEditProfileViewModel : EditProfile
{
    public MemberEditProfileViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public MemberEditProfileFormViewModel? FormViewModel { get; set; }

    public List<Point>? MemberPoints { get; set; }
}