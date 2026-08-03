using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ITSupportVerwaltungsTool_Demo.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string ErstelleToken(string benutzername)
    {
        var schluessel = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key fehlt in der Konfiguration.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, benutzername)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(schluessel));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expireMinutes = _config.GetValue<int?>("Jwt:ExpireMinutes") ?? 480;

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
