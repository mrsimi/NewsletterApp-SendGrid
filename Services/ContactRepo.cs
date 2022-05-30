using Microsoft.EntityFrameworkCore;
using NewsletterApp.Data;

namespace NewsletterApp.Services
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
            contact.Email = contact.Email.Trim().ToLower();
            _dbContext.Contacts.Add(contact);
            _dbContext.SaveChanges();
        }
        
        public void DeleteContact(Contact subscriber)
        {
            _dbContext.Contacts.Remove(subscriber);
            _dbContext.SaveChanges();
        }

        public int GetConfirmedContactsCount() => _dbContext.Contacts.Count();

        public List<Contact> GetConfirmedContacts(int pageSize, int page)
        {
            return _dbContext.Contacts
                .AsNoTracking()
                .Where(m => m.IsConfirmed == true)
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToList();
        }

        public Contact GetContactByEmail(string email)
        {
            email = email.Trim().ToLower();
            return _dbContext.Contacts
                .AsNoTracking()
                .FirstOrDefault(m => m.Email == email);
        }

        public Contact ConfirmContact(string email, Guid confirmationId)
        {
            var contact = _dbContext.Contacts.FirstOrDefault(m => m.Email == email.ToLower());
            if(contact != null && contact.ConfirmationId.Equals(confirmationId))
            {
                contact.IsConfirmed = true;
                _dbContext.SaveChanges();

                return contact;
            }

            return null;
        }
    }
}