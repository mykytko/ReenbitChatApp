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
    
    public string Issuer => _jwt["Issuer"];
    public string Audience => _jwt["Audience"];
    public SymmetricSecurityKey Key => new(Encoding.ASCII.GetBytes(_jwt["Key"]));
    public int Lifetime => int.Parse(_jwt["Lifetime"]);
}