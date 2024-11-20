using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;
using static Umbraco.Cms.Core.DependencyInjection.StaticServiceProvider;

namespace Wasabi.Helpers;

/// <summary>
///     Provides helper methods for retrieving image URLs for student members and companies.
/// </summary>
public class ImageHelpers
{
    private static UmbracoHelper UmbracoHelper => Instance.GetRequiredService<UmbracoHelper>();

    /// <summary>
    ///     Retrieves the profile image URL for a given student member.
    /// </summary>
    /// <param name="studentMember">The student member whose profile image URL is to be retrieved.</param>
    /// <param name="cropAlias">An optional crop alias to get a specific cropped version of the image.</param>
    /// <returns>
    ///     The URL of the profile image if it exists; otherwise, the URL of a placeholder image.
    ///     If neither is available, returns a data URL.
    /// </returns>
    public static string GetProfileImageUrl(StudentMember studentMember, string? cropAlias = null)
    {
        if (studentMember.ProfileImage != null)
            return (cropAlias != null
                ? studentMember.ProfileImage.GetCropUrl(cropAlias)
                : studentMember.ProfileImage.MediaUrl())!;

        IPublishedContent? placeHolder = UmbracoHelper.Media(Guid.Parse("2b4de618-f422-42ea-ad55-c499b2777dfb"));
        return placeHolder != null ? placeHolder.MediaUrl() : "data:,";
    }

    /// <summary>
    ///     Retrieves the company image URL for a given company.
    /// </summary>
    /// <param name="company">The company whose image URL is to be retrieved.</param>
    /// <returns>
    ///     The URL of the company image if it exists; otherwise, the URL of a placeholder image.
    ///     If neither is available, returns a data URL.
    /// </returns>
    public static string GetCompanyImageUrl(Company company)
    {
        if (company.CompanyLogo != null)
            return company.CompanyLogo.MediaUrl();

        IPublishedContent? placeHolder = UmbracoHelper.Media(Guid.Parse("ac7c0ac5-3d77-4b44-924e-8a5e319ff8bb\n"));
        return placeHolder != null ? placeHolder.MediaUrl() : "data:,";
    }
}