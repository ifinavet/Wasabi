using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.ViewModels.Member;

public class MemberEditProfileViewModel : EditProfile
{
    public MemberEditProfileViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }

    public MemberEditProfileFormViewModel FormViewModel { get; set; }
}