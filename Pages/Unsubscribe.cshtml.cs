using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Services;
using SendGrid;

namespace NewsletterApp.Pages;

public class UnsubscribeModel : PageModel
{
    private readonly IContactRepo _contactRepo;

    public string ResponseMessage { get; set; }

    public UnsubscribeModel(IContactRepo contactRepo)
    {
        _contactRepo = contactRepo;
    }

    public void OnGet(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] Guid confirmationId
    )
    {
        var contact = _contactRepo.GetContactByEmail(emailAddress);
        if (contact == null)
        {
            ResponseMessage = "Sorry, but it looks like you already unsubscribed.";
            return;
        }

        if (contact.ConfirmationId.Equals(confirmationId))
        {
            _contactRepo.DeleteContact(contact);
            ResponseMessage = "Thank you. You have been successfully removed from this subscriber list " +
                              "and won't receive any further emails from us.";
        }
        else
        {
            ResponseMessage = "Sorry, but this is an invalid link.";
        }
    }
}