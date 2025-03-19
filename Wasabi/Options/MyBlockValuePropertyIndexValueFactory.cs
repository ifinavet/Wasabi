using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Composing;

namespace Wasabi.Options;

public class MyBlockValuePropertyIndexValueFactory : DefaultPropertyIndexValueFactory,
    IBlockValuePropertyIndexValueFactory;

public class MyBlockValuePropertyIndexValueFactoryComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.Services.AddSingleton<IBlockValuePropertyIndexValueFactory, MyBlockValuePropertyIndexValueFactory>();
}
