using Umbraco.Cms.Core.Composing;

namespace Ifinavet.Web.Core.Services.CustomIndexing;

public class ExamineComposer : IComposer
{
    /// <summary>
    ///     Registers the changes to eventDate
    /// </summary>
    /// <param name="builder"></param>
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureExternalIndexOptions>();
    }
}