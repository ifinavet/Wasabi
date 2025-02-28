using Umbraco.Cms.Core.Composing;

namespace Wasabi.Options;

public class OptionsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<ReCaptchaModel>().Bind(builder.Config.GetSection("ReCaptcha"));
    }
}