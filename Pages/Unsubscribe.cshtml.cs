using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.DTO;
using NewsletterApp.Helpers;
using SendGrid;

namespace NewsletterApp.Pages
{
    public class UnsubscribeModel : PageModel
    {
        private readonly IConfiguration _config;
        public UnsubscribeModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        public async Task OnGetAsync(string query)
        {
             string decodedEmail = SecurityHelper.Base64Decode(query);
            if (new EmailAddressAttribute().IsValid(decodedEmail.ToString()))
            {
                string sendGridApiKey = _config["SendGridAPIKey"];
                var client = new SendGridClient(sendGridApiKey);

                var subscribeRequest = new SearchEmailQuery
                {
                    query = $"email LIKE '{decodedEmail}%'"
                };

                string data = JsonSerializer.Serialize(subscribeRequest);

                


                var response = await client.RequestAsync(
                    method: SendGridClient.Method.POST,
                    urlPath: "marketing/contacts/search",
                    requestBody: data
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Body.ReadFromJsonAsync<SubscribersResponse>();
                    if(responseJson.contact_count == 0)
                    {
                        ViewData["ResponseMessage"] = "You are already unsubscribed from this newsletter";
                    }
                    else
                    {
                        string userID = responseJson.result.FirstOrDefault().id;
                        var deleteResponse = await client.RequestAsync(
                            method: SendGridClient.Method.DELETE,
                            urlPath: $"marketing/contacts?ids={userID}"
                        );

                        if(deleteResponse.IsSuccessStatusCode)
                        {
                            ViewData["ResponseMessage"] = "We are sorry to see you go.";
                        }
                        else 
                        {
                            ViewData["ResponseMessage"]  = "There was an error while trying to unsubscribe you. Kindly try again later"
                        }
                    }
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