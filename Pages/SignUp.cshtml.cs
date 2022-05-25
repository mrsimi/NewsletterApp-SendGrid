using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages;

public class SignUpModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly ISendGridClient _sendGridClient;

    public SignUpModel(IConfiguration configuration, ISendGridClient sendGridClient)
    {
        _config = configuration;
        _sendGridClient = sendGridClient;
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
            From = new EmailAddress("home@turntablecharts.com", "Tan Business"),
            Subject = "Confirm Newsletter Signup",
            //TODO: remove this property or implement it properly like the HtmlContent
            PlainTextContent = "Welcome", 
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

        return RedirectToPage("SignUpSuccess");
    }
}