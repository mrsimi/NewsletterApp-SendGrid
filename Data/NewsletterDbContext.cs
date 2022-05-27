using Microsoft.EntityFrameworkCore;

namespace NewsletterApp_SendGrid.Data
{
    public class NewsletterDbContext : DbContext
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : base(options)
        {
        }

        public DbSet<Contact> Contacts {get; set;}
    }
}