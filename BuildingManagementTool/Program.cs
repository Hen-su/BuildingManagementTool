using BuildingManagementTool.Models;
using BuildingManagementTool.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnectionString:blob"]!, preferMsi: true);
});

builder.Services.AddDbContext<BuildingManagementToolDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:BuildingManagementToolDbContextConnection"]);
});

// Add BlobService class
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
