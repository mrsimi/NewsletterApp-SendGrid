using NewsletterApp_SendGrid.Data;

namespace NewsletterApp_SendGrid.Services
{
    public interface IContactRepo
    {
        void AddContact(Contact contact);
        Contact GetContactByEmail(string email);
        void ConfirmContact(int Id);
        List<Contact> GetConfirmedContacts();

        void DeleteContact(Contact contact);
    }
}