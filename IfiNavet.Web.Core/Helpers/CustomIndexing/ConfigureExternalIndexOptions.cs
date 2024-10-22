using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;

namespace Ifinavet.Web.Core.Helpers.CustomIndexing;

public class ConfigureExternalIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    /// <summary>
    /// Updates all eventDates fields to be a DateTime type instead of default text
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    public void Configure(string name, LuceneDirectoryIndexOptions options)
    {
        if (name.Equals(Constants.UmbracoIndexes.ExternalIndexName))
        {
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("eventDate", FieldDefinitionTypes.DateTime));
        }
    }

    /// <summary>
    /// Not needed inorder to update the field type
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(LuceneDirectoryIndexOptions options)
    {
        throw new System.NotImplementedException();
    }
}