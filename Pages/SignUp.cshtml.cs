using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IConfiguration _config;
        public SignUpModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        [BindProperty]
        public ContactInfo ContactInfo { get; set; }
        public IActionResult OnGet()
        {
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            string sendGridApiKey = _config["SendGridAPIKey"];
           

            var client = new SendGridClient(sendGridApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("home@turntablecharts.com", "Testing-Email"),
                Subject = "Confirm Newsletter Signup",
                PlainTextContent = "Welcome"
            };

            msg.AddTo(new EmailAddress(ContactInfo.Email, ContactInfo.FullName));
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./SignUpSuccess");
            }
            else
            {
                return RedirectToPage("./Error");
            }
        }
    }
}