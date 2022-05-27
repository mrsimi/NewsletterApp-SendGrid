using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Models;
using NewsletterApp_SendGrid.Data;
using NewsletterApp_SendGrid.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class SignUpModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly ISendGridClient _sendGridClient;
    private readonly IContactRepo _contactRepo;

    public SignUpModel(IConfiguration configuration, ISendGridClient sendGridClient, 
        IContactRepo contactRepo)
    {
        _config = configuration;
        _sendGridClient = sendGridClient;
        _contactRepo = contactRepo;
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

        var confirmationId = Guid.NewGuid();
        var confirmLink = Url.PageLink("Confirm", protocol: "https", values: new
        {
            email = SignUpViewModel.Email,
            confirmation = confirmationId
        });

        var message = new SendGridMessage
        {
            // TODO: pull email and name from configuration
            From = new EmailAddress(_config.GetValue<string>("SendGridSenderEmail"), _config.GetValue<string>("SendGridSenderName")),
            Subject = "Confirm Newsletter Signup",
            //TODO: remove this property or implement it properly like the HtmlContent
            HtmlContent = $"<h3>Hello {SignUpViewModel.FullName}</h3><p>Welcome to our Newsletter. <br> <br> " +
                          $"<br>Kindly click on the link below to confirm your subscription. <br>{confirmLink}</p>"
        };

        message.AddTo(new EmailAddress(SignUpViewModel.Email, SignUpViewModel.FullName));
        var response = await _sendGridClient.SendEmailAsync(message);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToPage("Error");
        }

        // TODO: create contact with name, email address, and confirmation ID
        var contact = new Contact
        {
            FullName = SignUpViewModel.FullName,
            Email = SignUpViewModel.Email,
            ConfirmationId = confirmationId
        };

        _contactRepo.AddContact(contact);

        return RedirectToPage("SignUpSuccess");
    }
}