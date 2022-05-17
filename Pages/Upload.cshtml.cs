using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp.Helpers;
using NewsletterApp.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterApp.Pages
{
    public class UploadModel : PageModel
    {
         private readonly IConfiguration _config;
        public UploadModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        [BindProperty]
        public UploadNewsletterInfo NewsletterInfo {get; set;}
        public void OnGet()
        {
            
        }   


        public async Task <IActionResult> OnPostAsync()
        {
            string sendGridApiKey = _config["SendGridAPIKey"];
            //read newsletter file to string
            string newsletterContent;
            using (var reader = new StreamReader(NewsletterInfo.Newsletter.OpenReadStream()))
            {
                newsletterContent = await reader.ReadToEndAsync();
            }
            newsletterContent = newsletterContent.Replace("\n", "").Replace("\r", "").Replace(@"\", "");

            
            

            //retrieve contacts from SendGrid and deserialize
             var client = new SendGridClient(sendGridApiKey);

            var response = await client.RequestAsync(
                    method: SendGridClient.Method.GET,
                    urlPath: "marketing/contacts"
            );


            // if there are subscribers iterate over all and send emails to them
            if(response.IsSuccessStatusCode)
            {
                var subscribersResponse = await response.Body.ReadFromJsonAsync<SubscribersResponse>();
                if(subscribersResponse.contact_count > 0)
                {
                    foreach (var subscriber in subscribersResponse.result)
                    {
                        string unsubscribelink = _config["WebsiteUrl"] + "unsubscribe/" + SecurityHelper.Base64Encode(subscriber.email);
                        var msg = new SendGridMessage
                        {
                            From = new EmailAddress("home@turntablecharts.com", "Tan Business"),
                            Subject = NewsletterInfo.EmailSubject,
                            PlainTextContent = NewsletterInfo.EmailSubject, 
                            HtmlContent = newsletterContent.Replace("{unsubscribelink}", unsubscribelink)
                        };

                        msg.AddTo(new EmailAddress(subscriber.email, string.Empty));
                        var sendEmailResponse = await client.SendEmailAsync(msg);
                    }

                    TempData["SuccessMessage"] = "Newsletter has been sent to all subscribers";
                }
                else 
                {
                    TempData["SuccessMessage"] = "There are currently no subscribers";
                }
            }
            else 
            {
                TempData["Error"] = "Sorry, an error has occured in retrieving your subscribers. <br> Try again";
            }

           
            
            return Page();
        }    
    }
}