using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp.Helpers;
using SendGrid;

namespace NewsletterApp.Pages
{
    public class ConfirmModel : PageModel
    {
        private readonly IConfiguration _config;
        public ConfirmModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        public async Task OnGetAsync(string query)
        {
            string decodedEmail = SecurityHelper.Base64Decode(query);
            if (new EmailAddressAttribute().IsValid(decodedEmail.ToString()))
            {
                string sendGridApiKey = _config["SendGridAPIKey"];

                var subscribeRequest = new SubscribeEmailRequest
                {
                    contacts = new List<dynamic> {
                        new {email = decodedEmail}
                    }
                };

                string data = JsonSerializer.Serialize(subscribeRequest);

                var client = new SendGridClient(sendGridApiKey);


                var response = await client.RequestAsync(
                    method: SendGridClient.Method.PUT,
                    urlPath: "marketing/contacts",
                    requestBody: data
                );

                if (response.IsSuccessStatusCode)
                {
                    ViewData["ResponseMessage"] = "Thank you for Signing up for our newsletter.";
                }
                else
                {
                    ViewData["ResponseMessage"] = "Sorry, but this is an invalid link";
                }

            }
            else
            {
                ViewData["ResponseMessage"] = "Sorry, but this is an invalid link";
            }
        }
    }
}