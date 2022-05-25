using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid;

namespace NewsletterApp.Pages;

public class ConfirmModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;

    public string ResponseMessage { get; set; }

    public ConfirmModel(ISendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient;
    }

    public async Task OnGetAsync(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] Guid confirmationId
    )
    {
        // TODO: retrieve existing contact from contact database
        // TODO: verify confirmation ID matches from the ID stored in SendGrid contact
        // TODO: don't create a contact, add existing contact to marketing list

        // Suggestion: use anonymous objects for requests, keeps things simpler for the tutorial
        var subscribeRequest = new
        {
            contacts = new[]
            {
                new {email = emailAddress}
            }
        };

        string requestBody = JsonSerializer.Serialize(subscribeRequest);

        var response = await _sendGridClient.RequestAsync(
            method: SendGridClient.Method.PUT,
            urlPath: "marketing/contacts",
            requestBody: requestBody
        );

        if (response.IsSuccessStatusCode)
        {
            ResponseMessage = "Thank you for Signing up for our newsletter.";
        }
        else
        {
            ResponseMessage = "Sorry, but this is an invalid link";
        }
    }
}