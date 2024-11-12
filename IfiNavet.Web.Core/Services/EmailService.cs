using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace IfiNavet.Web.Core.Services;

public interface IEmailService
{
    bool SendEmail(string emailTemplateName, string toEmail, Dictionary<string, string> fields);
}

public class EmailService : IEmailService
{
    private readonly GlobalSettings _globalSettings;
    private readonly IPublishedContentQuery _contentQuery;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<GlobalSettings> globalSettings,
        IPublishedContentQuery publishedContentQuery,
        ILogger<EmailService> logger)
    {
        _globalSettings = globalSettings.Value;
        _contentQuery = publishedContentQuery;
        _logger = logger;
    }

    /// <summary>
    /// Sends an email using the specified email template and fields.
    /// </summary>
    /// <param name="emailTemplateName">The name of the email template to use.</param>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="fields">A dictionary of fields to replace in the email template.</param>
    /// <returns>
    /// <c>true</c> if the email was sent successfully; otherwise, <c>false</c>.
    /// </returns>
    public bool SendEmail(string emailTemplateName, string toEmail,
        Dictionary<string, string> fields)
    {
        if (!_globalSettings.IsSmtpServerConfigured)
        {
            _logger.LogDebug("SMTP is not configured");
            return false;
        }

        SiteSettings? siteSettings =
            _contentQuery.ContentAtRoot().First(f => f.ContentType.Alias == "siteSettings") as SiteSettings;

        if (siteSettings?.Children.First(child => child.Name == emailTemplateName) is not EmailTemplate emailTemplate)
            return false;

        string? emailMasterTemplate = siteSettings.MasterEmailTemplate;
        string? emailBodyTemplate = emailTemplate.Body;
        string? emailSubject = emailTemplate.Subject;
        string? emailBody = emailMasterTemplate?.Replace("[[CONTENT]]", emailBodyTemplate);

        try
        {
            SmtpClient smtpClient = new(_globalSettings.Smtp!.Host, _globalSettings.Smtp.Port);

            if (!string.IsNullOrWhiteSpace(_globalSettings.Smtp.Username) &&
                !string.IsNullOrWhiteSpace(_globalSettings.Smtp.Password))
                smtpClient.Credentials =
                    new NetworkCredential(_globalSettings.Smtp.Username, _globalSettings.Smtp.Password);
            else
                smtpClient.UseDefaultCredentials = true;

            MailMessage message = new()
            {
                From = new MailAddress(_globalSettings.Smtp.From, "Ifinavet : Ifinavet.no"),
                Subject = ReplaceFields(emailSubject!, fields),
                Body = ReplaceFields(emailBody!, fields),
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            smtpClient.Send(message);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Error trying to send email {e}", e);
            return false;
        }
    }

    /// <summary>
    /// Replaces placeholders in the input text with corresponding values from the dictionary.
    /// </summary>
    /// <param name="textIn">The input text containing placeholders.</param>
    /// <param name="emailFields">A dictionary where keys are placeholders and values are the replacements.</param>
    /// <returns>The input text with placeholders replaced by their corresponding values.</returns>
    private static string ReplaceFields(string textIn, Dictionary<string, string> emailFields)
    {
        return emailFields.Aggregate(textIn,
            (current, emailField) =>
                current.Replace(string.Concat("[[", emailField.Key.ToUpper(), "]]"), emailField.Value));
    }
}