using Newtonsoft.Json;

namespace Wasabi.Services;

public interface IGoogleReCaptchaService
{
    Task<bool> VerifyCaptcha(string responseToken);
}

public class GoogleReCaptchaService : IGoogleReCaptchaService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleReCaptchaService> _logger;

    public GoogleReCaptchaService(
        IConfiguration configuration,
        ILogger<GoogleReCaptchaService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> VerifyCaptcha(string responseToken)
    {
        const string verificationUrl = "https://www.google.com/recaptcha/api/siteverify";
        string secretKey = _configuration["reCaptcha:reCaptchaSecretKey"]!;

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