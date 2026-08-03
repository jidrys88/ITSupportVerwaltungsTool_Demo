using ITSupportVerwaltungsTool_Demo.Data;
using ITSupportVerwaltungsTool_Demo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSupportVerwaltungsTool_Demo.Controllers;

public record LoginAnfrage(string Benutzername, string Passwort);
public record LoginAntwort(string Token, string Benutzername);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<object> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginAntwort>> Login(LoginAnfrage anfrage)
    {
        if (string.IsNullOrWhiteSpace(anfrage.Benutzername) || string.IsNullOrWhiteSpace(anfrage.Passwort))
        {
            return BadRequest("Benutzername und Passwort werden benötigt.");
        }

        var benutzer = await _db.Benutzer
            .FirstOrDefaultAsync(b => b.Benutzername == anfrage.Benutzername);

        if (benutzer is null)
        {
            return Unauthorized("Benutzername oder Passwort ist falsch.");
        }

        var ergebnis = _hasher.VerifyHashedPassword(new object(), benutzer.PasswortHash, anfrage.Passwort);
        if (ergebnis == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Benutzername oder Passwort ist falsch.");
        }

        var token = _tokenService.ErstelleToken(benutzer.Benutzername);
        return Ok(new LoginAntwort(token, benutzer.Benutzername));
    }
}
