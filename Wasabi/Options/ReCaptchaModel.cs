namespace Wasabi.Options;

public class ReCaptchaModel
{
    public required string ReCaptchaSecretKey { get; init; }
    public required string ReCaptchaSiteKey { get; init; }
}