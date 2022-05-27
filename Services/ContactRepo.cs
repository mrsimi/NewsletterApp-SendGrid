using NewsletterApp_SendGrid.Data;

namespace NewsletterApp_SendGrid.Services
{
    public class ContactRepo : IContactRepo
    {
        private readonly NewsletterDbContext _dbContext;
        public ContactRepo(NewsletterDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void AddContact(Contact contact)
        {
           
            contact.Email = contact.Email.ToLower();
            _dbContext.Contacts.Add(contact);
            _dbContext.SaveChanges();
        }

        public void ConfirmContact(int Id)
        {
           var subscriber = _dbContext.Contacts.FirstOrDefault(m => m.Id == Id);
           subscriber.IsConfirmed = true; 
           _dbContext.SaveChanges();
        }

        public void DeleteContact(Contact subscriber)
        {
            _dbContext.Contacts.Remove(subscriber);
            _dbContext.SaveChanges();
        }

        public List<Contact> GetConfirmedContacts()
        {
            return _dbContext.Contacts.Where(m => m.IsConfirmed == true).ToList();
        }

        public Contact GetContactByEmail(string email)
        {
            email = email.Trim().ToLower();
            return _dbContext.Contacts.FirstOrDefault(m => m.Email == email);
        }
    }
}