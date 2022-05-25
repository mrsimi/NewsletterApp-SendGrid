using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using SendGrid;

namespace NewsletterApp.Pages;

public class UnsubscribeModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;

    public string ResponseMessage { get; set; }

    public UnsubscribeModel(ISendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient;
    }

    public async Task OnGetAsync(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] Guid confirmationId
    )
    {
        var subscribeRequest = new
        {
            emails = new[] {emailAddress}
        };

        string requestBody = JsonSerializer.Serialize(subscribeRequest);

        var response = await _sendGridClient.RequestAsync(
            method: SendGridClient.Method.POST,
            urlPath: "marketing/contacts/search/emails",
            requestBody: requestBody
        );

        if (!response.IsSuccessStatusCode)
        {
            //TODO: use an error message that's more accurate
            ResponseMessage = "Sorry, but this is an invalid link";
        }

        var responseJson = await response.Body.ReadFromJsonAsync<SubscribersResponse>();
        //TODO: verify `confirmationId` matches the confirmation number you stored in the SendGrid contact
        //TODO: remove contact from Marketing List
    }
}