using Microsoft.EntityFrameworkCore;
using NewsletterApp_SendGrid.Data;
using NewsletterApp_SendGrid.Services;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//Add Dbcontext
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
