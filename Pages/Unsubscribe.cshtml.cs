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
                    string userID = responseJson.result.FirstOrDefault().id;
                    // var deleteData = new DeleteEmailRequest
                    // {
                    //     ids = userID
                    // };

                    // string jsonString = JsonSerializer.Serialize(deleteData);

                    var queryParams = @"{
                        'ids': 'value'
                    }";

                    queryParams = queryParams.Replace("value", userID);

                    var deleteResponse = await client.RequestAsync(
                        method: SendGridClient.Method.DELETE,
                        urlPath: "marketing/contacts",
                        requestBody: queryParams
                    );



                    ViewData["ResponseMessage"] = "We are sorry to see you go.";
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