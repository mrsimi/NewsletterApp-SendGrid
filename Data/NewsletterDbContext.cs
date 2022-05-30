using Microsoft.EntityFrameworkCore;

namespace NewsletterApp.Data
{
    public class NewsletterDbContext : DbContext
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }

        public DbSet<Contact> Contacts {get; set;}
    }
}