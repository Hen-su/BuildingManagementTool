using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Twitter;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BuildingManagementToolDbContextConnection") ?? throw new InvalidOperationException("Connection string 'BuildingManagementToolDbContextConnection' not found.");

// Define the config object to access configuration settings
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(config["StorageConnectionString:blob"]!, preferMsi: true);
});

builder.Services.AddDbContext<BuildingManagementToolDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<IdentityRole>() // Add this line to include roles
    .AddEntityFrameworkStores<BuildingManagementToolDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add BlobService class
builder.Services.AddScoped<IBlobService, BlobService>();
// Add Model Repositories
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyCategoryRepository, PropertyCategoryRepository>();
builder.Services.AddScoped<IUserPropertyRepository, UserPropertyRepository>();
builder.Services.AddScoped<IPropertyImageRepository, PropertyImageRepository>();
// Add Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("AuthMessageSenderOptions"));
builder.Services.AddScoped<RazorViewToStringRenderer>();

// Add external authentication providers
builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection =
       config.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
   })
   .AddFacebook(options =>
   {
       IConfigurationSection FBAuthNSection =
       config.GetSection("Authentication:FB");
       options.ClientId = FBAuthNSection["ClientId"];
       options.ClientSecret = FBAuthNSection["ClientSecret"];
   })
   .AddTwitter(twitterOptions =>
   {
       twitterOptions.ConsumerKey = config["Authentication:Twitter:ConsumerAPIKey"];
       twitterOptions.ConsumerSecret = config["Authentication:Twitter:ConsumerSecret"];
       twitterOptions.RetrieveUserDetails = true;
   });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use session before authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

DbInitializer.Seed(app);

app.MapRazorPages();

app.Run();
