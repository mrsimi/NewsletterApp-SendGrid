using Microsoft.EntityFrameworkCore;
using NewsletterApp.Data;
using NewsletterApp.Services;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext
builder.Services.AddDbContext<NewsletterDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSendGrid(options =>
    options.ApiKey = builder.Configuration["SendGridApiKey"]
);

builder.Services.AddScoped<IContactRepo, ContactRepo>();


var app = builder.Build();

if (bool.Parse(app.Configuration["Seed:Run"] ?? "false"))
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<NewsletterDbContext>();
    var seeder = new NewsletterDbContextSeeder(dbContext, app.Configuration);
    await seeder.Run();
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
