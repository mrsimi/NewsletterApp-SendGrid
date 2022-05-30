using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Services;

namespace NewsletterApp.Pages;

public class ConfirmModel : PageModel
{
    private readonly IContactRepo _contactRepo;

    public string ResponseMessage { get; set; }

    public ConfirmModel(IContactRepo contactRepo)
    {
        _contactRepo = contactRepo;
    }

    public void OnGet(
        [FromQuery(Name = "email")] string emailAddress,
        [FromQuery(Name = "confirmation")] Guid confirmationId
    )
    {
        var savedContact = _contactRepo.ConfirmContact(emailAddress, confirmationId);
        if(savedContact == null)
        {
            ResponseMessage = "Sorry, but this is an invalid link.";
            return;
        }
        else 
        {
            ResponseMessage = "Thank you for signing up for our newsletter."; 
            return;
        }
    }
}