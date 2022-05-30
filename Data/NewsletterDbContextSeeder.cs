using Microsoft.EntityFrameworkCore;

namespace NewsletterApp.Data;

public class NewsletterDbContextSeeder
{
    private readonly NewsletterDbContext _context;
    private readonly IConfiguration _configuration;

    public NewsletterDbContextSeeder(NewsletterDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task Run()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Contacts]");
        
        var emailAddressTemplate = _configuration["Seed:EmailAddressTemplate"];
        var fullNameTemplate = _configuration["Seed:FullNameTemplate"];

        for (int i = 0; i < 2345; i++)
        {
            var emailAddress = string.Format(emailAddressTemplate, i);
            var fullName = string.Format(fullNameTemplate, i);
            _context.Contacts.Add(new Contact
            {
                Email = emailAddress,
                FullName = fullName,
                ConfirmationId = Guid.NewGuid(),
                IsConfirmed = true
            });
        }

        await _context.SaveChangesAsync();
    }
}