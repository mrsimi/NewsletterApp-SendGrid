using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp.Models;
using NewsletterApp_SendGrid.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class UploadModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IContactRepo _contactRepo;
    private readonly IConfiguration _config;

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    [BindProperty] public UploadNewsletterViewModel NewsletterViewModel { get; set; }

    public UploadModel(ISendGridClient sendGridClient, HtmlEncoder htmlEncoder, 
        IContactRepo contactRepo, IConfiguration config)
    {
        _sendGridClient = sendGridClient;
        _htmlEncoder = htmlEncoder;
        _contactRepo = contactRepo;
        _config = config;
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
        // I realized when an html file is read the formatting is not kept. 
        //For example a newline would have the \n character, and some \ characters where inserted at html class naming 
        newsletterContentBuilder.Replace("\n", "").Replace("\r", "").Replace(@"\", "");

        // create unsubscribeUrl with substitution tags
        string unsubscribeUrl = $"{Url.PageLink("Unsubscribe")}?email=userEmail&confirmation=userConfirmationId";
        newsletterContentBuilder.Replace("{UnsubscribeLink}", $@"<a href=""{unsubscribeUrl}"">Unsubscribe</a>");

       

        var contacts = _contactRepo.GetConfirmedContacts();
        if(contacts.Count == 0)
        {
            SuccessMessage = "There are currently no subscribers";
            return Page();
        }

        var message = new SendGridMessage
        {
            Personalizations = contacts.Select(subscriber => new Personalization
            {
                Tos = new List<EmailAddress> {new(subscriber.Email)},
                Substitutions = new Dictionary<string, string>
                {
                    {"userEmail", _htmlEncoder.Encode(subscriber.Email)}, // HTML encode to prevent HTML injection attacks
                    {"userConfirmationId", _htmlEncoder.Encode(subscriber.ConfirmationId.ToString())} 
                }
            }).ToList(),

            
            From = new EmailAddress(_config.GetValue<string>("SendGridSenderEmail"), _config.GetValue<string>("SendGridSenderName")),
            Subject = NewsletterViewModel.EmailSubject,
            HtmlContent = newsletterContentBuilder.ToString(),
            // Suggestion: add sendAt property to schedule newsletter at a certain time
        };

        var sendEmailResponse = await _sendGridClient.SendEmailAsync(message);
        //TODO: check success email response and provide accurate error messages

        if(!sendEmailResponse.IsSuccessStatusCode)
        {
            ErrorMessage = "Sorry, there was a problem while trying to send the newsletters. \n Try again later";
            return Page();
        }
        

        SuccessMessage = "Newsletter has been sent to all subscribers";
        return Page();
    }
}