using System.Text;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReenbitChatApp;
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

// Get KeyVault access
var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ManagedIdentityClientId = clientId
}), clientOptions);

// Get connection string from KeyVault
var connString = client.GetSecret("ChatDbConnection").Value.Value;

// Initialize database (i.e. delete and create anew with initial data)
// DbInitializer.Initialize(connString);

// Set up DbContextPool
builder.Services.AddDbContextPool<AppDbContext>(o => o.UseSqlServer(connString));

builder.Services.AddSignalR().AddAzureSignalR(env["SignalrConnectionString"]);

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
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (!path.StartsWithSegments("/chat"))
                {
                    return Task.CompletedTask;
                }
                
                var accessToken = context.Request.Query["access_token"];
                context.Token = accessToken;

                return Task.CompletedTask;
            }
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

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat");
});

app.MapFallbackToFile("wwwroot/index.html");

app.Run();