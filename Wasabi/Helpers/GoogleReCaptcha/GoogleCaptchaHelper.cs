using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;

namespace Wasabi.Helpers.GoogleReCaptcha;

public static class GoogleCaptchaHelper
{
    public static IHtmlContent ReCaptchaHidden()
    {
        string? publicSiteKey = GoogleCaptchaVariables.GoogleRecaptchaSiteKey;

        TagBuilder mvcHtmlString = new("input")
        {
            Attributes =
            {
                new KeyValuePair<string, string>("type", "hidden")!,
                new KeyValuePair<string, string?>("id", GoogleCaptchaVariables.InputName),
                new KeyValuePair<string, string?>("name", GoogleCaptchaVariables.InputName)
            }
        };
        string? renderedReCaptchaInput = mvcHtmlString.ToString();

        return new HtmlString($"{renderedReCaptchaInput}");
    }

    public static IHtmlContent ReCaptchaJavascript(string formId = "form", string useCase = "startpage")
    {
        string? reCaptchaSiteKey = GoogleCaptchaVariables.GoogleRecaptchaSiteKey;
        string reCaptchaApiScript =
            $"<script src='https://www.google.com/recaptcha/api.js?render={reCaptchaSiteKey}'></script>";

        string reCaptchaTokenResponseScript =
            $"<script>document.addEventListener('DOMContentLoaded', () => {{ document.getElementById('{formId}').addEventListener('submit', e => {{ e.preventDefault(); grecaptcha.ready(function() {{ grecaptcha.execute('{reCaptchaSiteKey}', {{action: '{useCase}'}}).then(function(token) {{ document.getElementById('{GoogleCaptchaVariables.InputName}').value = token; document.getElementById('{formId}').submit(); }}); }}); }}); }})</script>";

        HtmlString reCaptchaJs = new($"{reCaptchaApiScript}{reCaptchaTokenResponseScript}");
        return reCaptchaJs;
    }

    public static IHtmlContent ReCaptchaValidationMessage(this IHtmlHelper helper, string? errorText = "")
    {
        string? invalidReCaptcha = helper.ViewContext.TempData["InvalidCaptcha"]!.ToString();
        if (string.IsNullOrWhiteSpace(invalidReCaptcha)) return new HtmlString("");
        TagBuilder buttonTag = new("span")
        {
            Attributes =
            {
                new KeyValuePair<string, string?>("class", "text-danger")
            }
        };
        buttonTag.InnerHtml.Append(errorText ?? invalidReCaptcha);
        return new HtmlString(buttonTag.ToString());
    }
}