using Microsoft.Extensions.Configuration;
using System.IO;

namespace Wasabi.Helpers.GoogleReCaptcha;

public static class GoogleCaptchaVariables
{
    public static readonly string? GoogleRecaptchaSecretKey;
    public static readonly string? GoogleRecaptchaSiteKey;
    public const string InputName = "g-recaptcha-response";

    static GoogleCaptchaVariables()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        GoogleRecaptchaSecretKey = configuration["reCaptcha:reCaptchaSecretKey"];
        GoogleRecaptchaSiteKey = configuration["reCaptcha:reCaptchaSiteKey"];
    }
}