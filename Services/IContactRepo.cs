using NewsletterApp.Data;

namespace NewsletterApp.Services
{
    public interface IContactRepo
    {
        void AddContact(Contact contact);
        Contact GetContactByEmail(string email);
        void ConfirmContact(int id);
        int GetConfirmedContactsCount();
        List<Contact> GetConfirmedContacts(int pageSize, int page);
        void DeleteContact(Contact contact);
    }
}