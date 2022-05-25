using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class UploadModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;
    private readonly HtmlEncoder _htmlEncoder;

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    [BindProperty] public UploadNewsletterViewModel NewsletterViewModel { get; set; }

    public UploadModel(ISendGridClient sendGridClient, HtmlEncoder htmlEncoder)
    {
        _sendGridClient = sendGridClient;
        _htmlEncoder = htmlEncoder;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        //read newsletter file to string
        StringBuilder newsletterContentBuilder = new();
        using (var reader = new StreamReader(NewsletterViewModel.Newsletter.OpenReadStream()))
        {
            newsletterContentBuilder.Append(await reader.ReadToEndAsync());
        }

        // TODO: add comment to explain why this is necessary
        newsletterContentBuilder.Replace("\n", "").Replace("\r", "").Replace(@"\", "");
        // create unsubscribeUrl with substitution tags
        string unsubscribeUrl = $"{Url.PageLink("Unsubscribe")}?email=-email-&confirmation=-confirmation-";
        newsletterContentBuilder.Replace("{UnsubscribeLink}", $@"<a href=""{unsubscribeUrl}"">Unsubscribe</a>");

        // TODO: get contacts from SendGrid marketing list instead of getting all contacts
        //retrieve contacts from SendGrid and deserialize
        var response = await _sendGridClient.RequestAsync(
            method: SendGridClient.Method.GET,
            urlPath: "marketing/contacts"
        );

        // if there are subscribers iterate over all and send emails to them
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Sorry, an error has occured in retrieving your subscribers. <br> Try again";
            return Page();
        }

        var subscribersResponse = await response.Body.ReadFromJsonAsync<SubscribersResponse>();
        if (subscribersResponse.contact_count == 0)
        {
            SuccessMessage = "There are currently no subscribers";
            return Page();
        }

        var message = new SendGridMessage
        {
            Personalizations = subscribersResponse.result.Select(subscriber => new Personalization
            {
                Tos = new List<EmailAddress> {new(subscriber.email)},
                Substitutions = new Dictionary<string, string>
                {
                    {"email", _htmlEncoder.Encode(subscriber.email)}, // HTML encode to prevent HTML injection attacks
                    {"confirmation", _htmlEncoder.Encode("")} //TODO: fetch confirmation number
                }
            }).ToList(),

            // TODO: replace with email address and name from configuration
            From = new EmailAddress("home@turntablecharts.com", "Tan Business"),
            Subject = NewsletterViewModel.EmailSubject,
            //TODO: remove PlainTextContent or provide a plain text version of the HTML email
            PlainTextContent = NewsletterViewModel.EmailSubject,
            HtmlContent = newsletterContentBuilder.ToString()
            // Suggestion: add sendAt property to schedule newsletter at a certain time
        };

        var sendEmailResponse = await _sendGridClient.SendEmailAsync(message);
        //TODO: check success email response and provide accurate error messages

        SuccessMessage = "Newsletter has been sent to all subscribers";
        return Page();
    }
}