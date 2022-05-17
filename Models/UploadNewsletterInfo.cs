namespace NewsletterApp.Models
{
    public class UploadNewsletterInfo
    {
        public string EmailSubject { get; set; }
        public IFormFile Newsletter { get; set; }
    }
}