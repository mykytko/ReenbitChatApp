using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _appDbContext;
    private readonly AuthOptions _authOptions;

    public AuthController(AppDbContext appDbContext, AuthOptions authOptions)
    {
        _appDbContext = appDbContext;
        _authOptions = authOptions;
    }

    [HttpPost, Route("/auth/login")]
    public IActionResult Login([FromBody] User user)
    {
        var identity = GetIdentity(user.Login, user.Password);
        if (identity == null)
        {
            return Unauthorized();
        }

        var now = DateTime.Now;
        var jwt = new JwtSecurityToken(
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audience,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromDays(_authOptions.Lifetime)),
            signingCredentials: new SigningCredentials(_authOptions.Key, SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new
        {
            access_token = encodedJwt,
            username = identity.Name
        };

        return Json(response);
    }

    private ClaimsIdentity? GetIdentity(string login, string password)
    {
        var user = _appDbContext.Users.FirstOrDefault(u => u.Login == login);
        if (user == default)
        {
            return null;
        }
        
        var hasher = new PasswordHasher<User>();
        var verificationResult = hasher.VerifyHashedPassword(new User
        {
            Login = login,
            Password = password
        }, user.Password, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, user.Login),
            new(ClaimsIdentity.DefaultRoleClaimType, string.Empty)
        };
        
        return new ClaimsIdentity(claims, "Token", 
            ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
    }
}