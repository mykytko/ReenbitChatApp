using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ReenbitChatApp;
using ReenbitChatApp.EFL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Set up KeyVault access
var clientOptions = new SecretClientOptions
{
    Retry =
    {
        Delay= TimeSpan.FromSeconds(2),
        MaxDelay = TimeSpan.FromSeconds(16),
        MaxRetries = 5,
        Mode = RetryMode.Exponential
    }
};
var keyVault = builder.Configuration.GetSection("KeyVault");
var vaultUri = keyVault["VaultUri"];
var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions 
    { ManagedIdentityClientId = keyVault["ManagedIdentityClientId"]}), clientOptions);
var connString = client.GetSecret("ChatDbConnection").Value.Value;

// Set up DbContextPool
builder.Services.AddDbContextPool<AppDbContext>(o => o.UseSqlServer(connString));

// Initialize database
// DbInitializer.Initialize(connString);

builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();