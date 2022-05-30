using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Models;
using NewsletterApp.Data;
using NewsletterApp.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class SignUpModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly ISendGridClient _sendGridClient;
    private readonly IContactRepo _contactRepo;
    private readonly HtmlEncoder _htmlEncoder;

    public SignUpModel(
        IConfiguration configuration, 
        ISendGridClient sendGridClient, 
        IContactRepo contactRepo,
        HtmlEncoder htmlEncoder)
    {
        _config = configuration;
        _sendGridClient = sendGridClient;
        _contactRepo = contactRepo;
        _htmlEncoder = htmlEncoder;
    }

    [BindProperty] public SignUpViewModel SignUpViewModel { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        Guid confirmationId;
        var savedContact = _contactRepo.GetContactByEmail(SignUpViewModel.Email.ToLower());
        if(savedContact != null)
        {
            confirmationId = savedContact.ConfirmationId;
        }
        else 
        {
            confirmationId = Guid.NewGuid();
        }
       
        var confirmLink = Url.PageLink("Confirm", protocol: "https", values: new
        {
            email = SignUpViewModel.Email,
            confirmation = confirmationId
        });

        var message = new SendGridMessage
        {
            From = new EmailAddress(
                email: _config.GetValue<string>("SendGridSenderEmail"), 
                name: _config.GetValue<string>("SendGridSenderName")
            ),
            Subject = "Confirm Newsletter Signup",
            HtmlContent = $@"<h3>Ahoy {_htmlEncoder.Encode(SignUpViewModel.FullName)}!</h3>
                            <p>Welcome to our Newsletter. <br>
                            Kindly click on the link below to confirm your subscription: <br>
                            <a href=""{confirmLink}"">Confirm your newsletter subscription</a></p>"
        };

        message.AddTo(new EmailAddress(SignUpViewModel.Email, SignUpViewModel.FullName));
        var response = await _sendGridClient.SendEmailAsync(message);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToPage("Error");
        }

        if(savedContact == null)
        {
            var contact = new Contact
            {
                FullName = SignUpViewModel.FullName,
                Email = SignUpViewModel.Email,
                ConfirmationId = confirmationId
            };
            _contactRepo.AddContact(contact);
        }
       

        return RedirectToPage("SignUpSuccess");
    }
}