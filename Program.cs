using System.Diagnostics;
using System.Text;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReenbitChatApp.Controllers;
using ReenbitChatApp.EFL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Set up KeyVault access
var clientOptions = new SecretClientOptions
{
    Retry =
    {
        Delay = TimeSpan.FromSeconds(2),
        MaxDelay = TimeSpan.FromSeconds(16),
        MaxRetries = 5,
        Mode = RetryMode.Exponential
    }
};
var keyVault = builder.Configuration.GetSection("KeyVault");
var vaultUri = keyVault["VaultUri"];
var clientId = keyVault["ManagedIdentityClientId"];
var env = builder.Configuration.GetSection("EnvironmentKeys");
Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", env["ClientId"]);
Environment.SetEnvironmentVariable("AZURE_TENANT_ID", env["TenantId"]);
Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", env["ClientSecret"]);
Console.WriteLine(env["ClientId"] + " " + env["TenantId"] + " " + env["ClientSecret"]);
var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ManagedIdentityClientId = clientId
}), clientOptions);
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
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Add(ServiceDescriptor.Singleton(new AuthOptions(jwtSection)));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSection["Key"]))
        };
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();