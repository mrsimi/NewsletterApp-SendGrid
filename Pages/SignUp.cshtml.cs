using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Helpers;
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
            string confirmLink = _config["WebsiteUrl"] + "confirm/" + SecurityHelper.Base64Encode(ContactInfo.Email);


            var client = new SendGridClient(sendGridApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("home@turntablecharts.com", "Testing-Email"),
                Subject = "Confirm Newsletter Signup",
                PlainTextContent = "Welcome", 
                HtmlContent = $"<h3>Hello {ContactInfo.FullName}</h3><p>Welcome to our Newsletter. <br> <br> "+
                    $"<br>Kindly click on the link below to confirm your subscription. <br>{confirmLink}</p>"
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