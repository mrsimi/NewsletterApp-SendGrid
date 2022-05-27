using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.Models;

public class UploadNewsletterViewModel
{
    [Required]
    [DataType(DataType.Text)]
    public string EmailSubject { get; set; }
    
    [Required]
    [DataType(DataType.Upload)]
    public IFormFile Newsletter { get; set; }
}