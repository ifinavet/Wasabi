using Microsoft.AspNetCore.Html;

namespace Wasabi.Helpers;

public static class ReCaptchaHelper
{
    public static IHtmlContent ReCaptchaScript(string? siteKey, string formId,
        string inputFieldName)
    {
        string apiScript = $"<script src=\"https://www.google.com/recaptcha/api.js?render={siteKey}\"></script>";
        string script =
            $"<script defer>document.getElementById('{formId}').addEventListener('submit', e => " +
            $"{{e.preventDefault();grecaptcha.ready(function () " +
            $"{{grecaptcha.execute('{siteKey}', {{action: 'submit'}}).then(function (token) " +
            $"{{{{document.getElementById('{inputFieldName}').value = token;" +
            $"document.getElementById('{formId}').submit();}}}});}});}})</script>";

        return new HtmlString($"{apiScript}{script}");
    }
}