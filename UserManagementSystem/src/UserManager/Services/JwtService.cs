using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserManager.Models;

namespace UserManager.Services;

public interface IJwtService
{
    Task<string> CreateJwt(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly SymmetricSecurityKey _jwtKey;


    public JwtService(IConfiguration config, UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
        // jwtKey is used for both encrypting and decrypting the JWT token
        _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"] ?? throw new InvalidOperationException()));
    }


    public async Task<string> CreateJwt(User user)
    {
        // Adding User Info to the Token
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        // Adding user Roles to the Token

        var roles = await _userManager.GetRolesAsync(user);
        userClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        // Generating Credentials
        var credentials = new SigningCredentials(_jwtKey, SecurityAlgorithms.HmacSha512Signature);
        // Generating Token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(userClaims),
            Expires = DateTime.UtcNow.AddDays(int.Parse(_config["JWT:ExpiresInDays"] ?? throw new InvalidOperationException())),
            SigningCredentials = credentials,
            Issuer = _config["JWT:Issuer"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(jwt);
    }

}

