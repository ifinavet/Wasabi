using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Wasabi.Services;

public interface IImageService
{
    public string GetProfileImageUrl(StudentMember studentMember, string? cropAlias = null);
    public string GetCompanyImageUrl(Company? company);
}

public class ImageService : IImageService
{
    private readonly UmbracoHelper _umbracoHelper;

    public ImageService(UmbracoHelper umbracoHelper)
    {
        _umbracoHelper = umbracoHelper;
    }

    /// <summary>
    ///     Retrieves the profile image URL for a given student member.
    /// </summary>
    /// <param name="studentMember">The student member whose profile image URL is to be retrieved.</param>
    /// <param name="cropAlias">An optional crop alias to get a specific cropped version of the image.</param>
    /// <returns>
    ///     The URL of the profile image if it exists; otherwise, the URL of a placeholder image.
    ///     If neither is available, returns a data URL.
    /// </returns>
    public string GetProfileImageUrl(StudentMember studentMember, string? cropAlias = null)
    {
        if (studentMember.ProfileImage != null && !string.IsNullOrEmpty(studentMember.ProfileImage.MediaUrl()))
            return (cropAlias != null
                ? studentMember.ProfileImage.GetCropUrl(cropAlias)
                : studentMember.ProfileImage.MediaUrl())!;

        string? placeHolder = _umbracoHelper.Media(1303)?.MediaUrl();
        return placeHolder ?? "data:,";
    }
    
    /// <summary>
    ///     Retrieves the company image URL for a given company.
    /// </summary>
    /// <param name="company">The company whose image URL is to be retrieved.</param>
    /// <returns>
    ///     The URL of the company image if it exists; otherwise, the URL of a placeholder image.
    ///     If neither is available, returns a data URL.
    /// </returns>
    public string GetCompanyImageUrl(Company? company)
    {
        if (company?.CompanyLogo != null)
            return company.CompanyLogo.MediaUrl();

        string? placeHolder = _umbracoHelper.Media(1339)?.MediaUrl();
        return placeHolder ?? "data:,";
    }
}