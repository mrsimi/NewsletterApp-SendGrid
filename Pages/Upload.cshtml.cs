using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Models;
using NewsletterApp.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class UploadModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IContactRepo _contactRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<UploadModel> _logger;

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    [BindProperty] public UploadNewsletterViewModel NewsletterViewModel { get; set; }

    public UploadModel(
        ISendGridClient sendGridClient,
        HtmlEncoder htmlEncoder,
        IContactRepo contactRepo,
        IConfiguration config,
        ILogger<UploadModel> logger)
    {
        _sendGridClient = sendGridClient;
        _htmlEncoder = htmlEncoder;
        _contactRepo = contactRepo;
        _config = config;
        _logger = logger;
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

        StringBuilder newsletterContentBuilder = new();

        // read newsletter file to string
        using (var reader = new StreamReader(NewsletterViewModel.Newsletter.OpenReadStream()))
        {
            newsletterContentBuilder.Append(await reader.ReadToEndAsync());
        }

        // Remove unnecessary newline and return characters 
        newsletterContentBuilder.Replace("\n", "").Replace("\r", "");

        // create unsubscribeUrl with substitution tags
        var unsubscribeUrl =
            $"{Url.PageLink("Unsubscribe")}?email=-SubscriberEmail-&confirmation=-SubscriberConfirmationId-";
        newsletterContentBuilder.Replace("{UnsubscribeLink}", $@"<a href=""{unsubscribeUrl}"">Unsubscribe</a>");

        var contactsCount = _contactRepo.GetConfirmedContactsCount();
        if (contactsCount == 0)
        {
            SuccessMessage = "There are currently no subscribers";
            return Page();
        }

        // paginate contacts by pageSize amount per page
        // to not load too many contacts into memory at one time
        const int pageSize = 1000;
        var amountOfPages = (int) Math.Ceiling((double) contactsCount / pageSize);

        for (var currentPage = 0; currentPage < amountOfPages; currentPage++)
        {
            var contacts = _contactRepo.GetConfirmedContacts(pageSize, currentPage);
            var message = new SendGridMessage
            {
                // max 1000 Personalizations per message!
                Personalizations = contacts.Select(subscriber => new Personalization
                {
                    Tos = new List<EmailAddress> {new(subscriber.Email)},
                    // total collective size of Substitutions may not exceed 10,000 bytes
                    Substitutions = new Dictionary<string, string>
                    {
                        // HTML encode to prevent HTML injection attacks
                        {"-SubscriberEmail-", _htmlEncoder.Encode(subscriber.Email)},
                        {"-SubscriberConfirmationId-", _htmlEncoder.Encode(subscriber.ConfirmationId.ToString())}
                    }
                }).ToList(),

                From = new EmailAddress(
                    email: _config.GetValue<string>("SendGridSenderEmail"),
                    name: _config.GetValue<string>("SendGridSenderName")
                ),
                Subject = NewsletterViewModel.EmailSubject,
                HtmlContent = newsletterContentBuilder.ToString(),
                // Suggestion: add sendAt property to schedule newsletter at a certain time
            };

            var response = await _sendGridClient.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to send Newsletter issue: {ResponseStatusCode} - {ResponseBody}",
                    response.StatusCode,
                    await response.Body.ReadAsStringAsync()
                );
                ErrorMessage = "Sorry, there was a problem while trying to send the newsletters. " +
                               "Some emails may have been send, see https://app.sendgrid.com/email_activity.";
                return Page();
            }
        }

        // Suggestion: create UploadSuccessPage or update Upload.cshtml to look better after successful submit
        SuccessMessage = "Newsletter issue has been sent to all subscribers";
        return Page();
    }
}