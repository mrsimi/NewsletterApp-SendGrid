using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp_SendGrid.Services;
using SendGrid;

namespace NewsletterApp.Pages;

public class ConfirmModel : PageModel
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IContactRepo _contactRepo;

    public string ResponseMessage { get; set; }

    public ConfirmModel(ISendGridClient sendGridClient, IContactRepo contactRepo)
    {
        _sendGridClient = sendGridClient;
        _contactRepo = contactRepo;
    }

    public void OnGet(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] Guid confirmationId
    )
    {
        // TODO: retrieve existing contact from contact database
        var savedContact = _contactRepo.GetContactByEmail(emailAddress);
        if(savedContact == null)
        {
            ResponseMessage = "Sorry, but this is an invalid link";
        }
        // TODO: verify confirmation ID matches from the ID stored in SendGrid contact
        if(savedContact.ConfirmationId.Equals(confirmationId))
        {
            _contactRepo.ConfirmContact(savedContact.Id);
            ResponseMessage = "Thank you for Signing up for our newsletter.";
        }
        else 
        {
            ResponseMessage = "Sorry, but this is an invalid link";   
        }
    }
}