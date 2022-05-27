namespace NewsletterApp_SendGrid.Data
{
    public class Contact
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid ConfirmationId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}