using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;

namespace Ifinavet.Web.Core.CustomIndexing;

public class ConfigureExternalIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    public void Configure(string name, LuceneDirectoryIndexOptions options)
    {
        if (name.Equals(Constants.UmbracoIndexes.ExternalIndexName))
        {
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("eventDate", FieldDefinitionTypes.DateTime));
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options)
    {
        throw new System.NotImplementedException();
    }
}