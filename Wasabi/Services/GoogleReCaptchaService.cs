using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wasabi.Options;

namespace Wasabi.Services;

public interface IGoogleReCaptchaService
{
    Task<bool> VerifyCaptcha(string responseToken);
}

public class GoogleReCaptchaService : IGoogleReCaptchaService
{
    private readonly ILogger<GoogleReCaptchaService> _logger;
    private readonly IOptions<ReCaptchaModel> _reCaptchaOptions;

    public GoogleReCaptchaService(
        ILogger<GoogleReCaptchaService> logger,
        IOptions<ReCaptchaModel> reCaptchaOptions)
    {
        _logger = logger;
        _reCaptchaOptions = reCaptchaOptions;
    }

    public async Task<bool> VerifyCaptcha(string responseToken)
    {
        const string verificationUrl = "https://www.google.com/recaptcha/api/siteverify";
        string secretKey = _reCaptchaOptions.Value.reCaptchaSecretKey;

        using HttpClient client = new();
        MultipartFormDataContent content = new();
        content.Add(new StringContent(responseToken), "response");
        content.Add(new StringContent(secretKey), "secret");

        HttpResponseMessage result = await client.PostAsync(verificationUrl, content);
        if (!result.IsSuccessStatusCode) return false;

        string resultString = await result.Content.ReadAsStringAsync();
        ResponseToken? response = JsonConvert.DeserializeObject<ResponseToken>(resultString);

        if (response is null) return false;

        if (response.Success != true)
        {
            _logger.LogWarning($"ReCaptcha verification failed. Host: {response.HostName}");
            return false;
        }

        if (response.Score < 0.5f)
        {
            _logger.LogWarning($"ReCaptcha score to low. Host: {response.HostName}");
            return false;
        }

        return true;
    }

    internal class ResponseToken
    {
        public bool Success { get; set; }
        public float Score { get; set; }
        public string? Action { get; set; }
        public DateTime ChallengeTs { get; set; }
        public string? HostName { get; set; }
        public List<string>? ErrorCodes { get; set; }
    }
}