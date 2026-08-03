using ITSupportVerwaltungsTool_Demo.Data;
using ITSupportVerwaltungsTool_Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSupportVerwaltungsTool_Demo.Controllers;

public record KundeDto(string Name, string? Adresse, string? Telefonnummer, string? Email);

[ApiController]
[Authorize]
[Route("api/kunden")]
public class KundenController : ControllerBase
{
    private readonly AppDbContext _db;

    public KundenController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Kunde>>> Liste([FromQuery] string? suche)
    {
        var query = _db.Kunden.AsQueryable();

        if (!string.IsNullOrWhiteSpace(suche))
        {
            query = query.Where(k => k.Name.Contains(suche));
        }

        var kunden = await query.OrderBy(k => k.Name).ToListAsync();
        return Ok(kunden);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Kunde>> Details(int id)
    {
        var kunde = await _db.Kunden.FindAsync(id);
        if (kunde is null) return NotFound();
        return Ok(kunde);
    }

    [HttpPost]
    public async Task<ActionResult<Kunde>> Erstellen(KundeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        var kunde = new Kunde
        {
            Name = dto.Name,
            Adresse = dto.Adresse,
            Telefonnummer = dto.Telefonnummer,
            Email = dto.Email
        };

        _db.Kunden.Add(kunde);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details), new { id = kunde.Id }, kunde);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Bearbeiten(int id, KundeDto dto)
    {
        var kunde = await _db.Kunden.FindAsync(id);
        if (kunde is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        kunde.Name = dto.Name;
        kunde.Adresse = dto.Adresse;
        kunde.Telefonnummer = dto.Telefonnummer;
        kunde.Email = dto.Email;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Loeschen(int id)
    {
        var kunde = await _db.Kunden.FindAsync(id);
        if (kunde is null) return NotFound();

        _db.Kunden.Remove(kunde);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
