using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp_SendGrid.Services;
using SendGrid;

namespace NewsletterApp.Pages;

public class UnsubscribeModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IContactRepo _contactRepo;

    public string ResponseMessage { get; set; }

    public UnsubscribeModel(ISendGridClient sendGridClient, IContactRepo contactRepo)
    {
        _sendGridClient = sendGridClient;
        _contactRepo = contactRepo;
    }

    public void OnGet(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] string confirmation
    )
    {
        var confirmationId = new Guid(confirmation);
        var contact = _contactRepo.GetContactByEmail(emailAddress);
        if (contact == null)
        {
            ResponseMessage = "Sorry, but it looks like you already unsubscribed";
        }
        else
        {
            if (contact.ConfirmationId.Equals(confirmationId))
            {
                _contactRepo.DeleteContact(contact);
                ResponseMessage = "Thank you \n You have been successfully " +
                "removed from this subscriber list and \n won't recieve any further emails from us \n\n";
            }
            else
            {
                ResponseMessage = "Invalid link \n\n  " +
                "Sorry, we cannot unsubscribe you because this links appears to be corrupted";
            }

        }
    }
}