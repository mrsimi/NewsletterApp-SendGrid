using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.Models;

public class SignUpViewModel
{
    [Required]
    [DataType(DataType.Text)]
    public string FullName { get; set; }
    
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}