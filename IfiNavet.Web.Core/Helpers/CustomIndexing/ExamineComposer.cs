using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Ifinavet.Web.Core.Helpers.CustomIndexing;

public class ExamineComposer : IComposer
{
    /// <summary>
    /// Registers the changes to eventDate
    /// </summary>
    /// <param name="builder"></param>
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureExternalIndexOptions>();
    }
}