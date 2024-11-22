using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Wasabi.Helpers.GoogleReCaptcha;

public class ValidateReCaptchaAttribute : ActionFilterAttribute
{
    private const float CaptchaScoreThreshold = 0.5f;

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        string? reCaptchaToken = filterContext.HttpContext.Request.Form[GoogleCaptchaVariables.InputName];
        string reCaptchaResponse = ReCaptchaVerify(reCaptchaToken);
        ResponseToken? response = JsonConvert.DeserializeObject<ResponseToken>(reCaptchaResponse);

        if (response?.Success != true)
        {
            AddErrorAndRedirectToGetAction(filterContext);
        }

        if (response?.Score < CaptchaScoreThreshold)
        {
            AddErrorAndRedirectToGetAction(filterContext);
        }

        base.OnActionExecuting(filterContext);
    }

    private static string ReCaptchaVerify(string? responseToken)
    {
        const string apiAddress = "https://www.google.com/recaptcha/api/siteverify";
        string? recaptchaSecretKey = GoogleCaptchaVariables.GoogleRecaptchaSecretKey;
        string urlToPost = $"{apiAddress}?secret={recaptchaSecretKey}&response={responseToken}";
        string responseString = "";
        using HttpClient httpClient = new();
        try
        {
            responseString = httpClient.GetStringAsync(urlToPost).Result;
        }
        catch
        {
            Console.WriteLine(typeof(ValidateReCaptchaAttribute) + " Error while verifying reCaptcha token");
        }

        return responseString;
    }

    private static void AddErrorAndRedirectToGetAction(ActionExecutingContext filterContext, string message = null)
    {
        ((Controller)filterContext.Controller).TempData["InvalidCaptcha"] = message ??
                                                                            "Invalid Captcha! The form cannot be submitted.";
        filterContext.Result = new ContentResult
        {
            Content =
                "Hi there hackerboi! ☺ hvis du ikke er en bot eller hacker " +
                "- ta kontakt med ifinavet eller prøv å laste siden på nytt :)",
            ContentType = "text/plain; charset=utf-8"
        };
        //TODO: what do we do with people who fail the captcha?
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