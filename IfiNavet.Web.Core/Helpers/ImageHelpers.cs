using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Common;

namespace IfiNavet.Web.Core.Helpers;

public class ImageHelpers
{
    private static UmbracoHelper UmbracoHelper => StaticServiceProvider.Instance.GetRequiredService<UmbracoHelper>();
    public static string GetProfileImageUrl(StudentMember studentMember, string? cropAlias = null)
    {
        if (studentMember.ProfileImage != null)
            return (cropAlias != null ? studentMember.ProfileImage.GetCropUrl(cropAlias) : studentMember.ProfileImage.MediaUrl())!;

        IPublishedContent? placeHolder = UmbracoHelper.Media(Guid.Parse("2b4de618-f422-42ea-ad55-c499b2777dfb"));
        return placeHolder != null ? placeHolder.MediaUrl() : "data:,";
    }

    public static string GetCompanyImageUrl(Company company)
    {
        if (company.CompanyLogo != null)
            return company.CompanyLogo.MediaUrl();

        IPublishedContent? placeHolder = UmbracoHelper.Media(Guid.Parse("ac7c0ac5-3d77-4b44-924e-8a5e319ff8bb\n"));
        return placeHolder != null ? placeHolder.MediaUrl() : "data:,";
    }
}