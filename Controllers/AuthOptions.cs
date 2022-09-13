using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ReenbitChatApp.Controllers;

public class AuthOptions
{
    private readonly IConfigurationSection _jwt;

    public AuthOptions(IConfigurationSection jwt)
    {
        _jwt = jwt;
    }
    
    public string Issuer
    {
        get => _jwt["Issuer"];
    }

    public string Audience
    {
        get => _jwt["Audience"];
    }

    public SymmetricSecurityKey Key
    {
        get => new(Encoding.ASCII.GetBytes(_jwt["Key"]));
    }

    public int Lifetime
    {
        get => int.Parse(_jwt["Lifetime"]);
    }
}