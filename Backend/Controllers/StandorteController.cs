using ITSupportVerwaltungsTool_Demo.Data;
using ITSupportVerwaltungsTool_Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSupportVerwaltungsTool_Demo.Controllers;

public record StandortDto(string Name, string? Adresse);

[ApiController]
[Authorize]
[Route("api/kunden/{kundeId}/standorte")]
public class StandorteController : ControllerBase
{
    private readonly AppDbContext _db;

    public StandorteController(AppDbContext db)
    {
        _db = db;
    }

    private async Task<bool> KundeExistiert(int kundeId) =>
        await _db.Kunden.AnyAsync(k => k.Id == kundeId);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Standort>>> Liste(int kundeId)
    {
        if (!await KundeExistiert(kundeId)) return NotFound("Kunde nicht gefunden.");

        var standorte = await _db.Standorte
            .Where(s => s.KundeId == kundeId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(standorte);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Standort>> Details(int kundeId, int id)
    {
        var standort = await _db.Standorte.FirstOrDefaultAsync(s => s.Id == id && s.KundeId == kundeId);
        if (standort is null) return NotFound();
        return Ok(standort);
    }

    [HttpPost]
    public async Task<ActionResult<Standort>> Erstellen(int kundeId, StandortDto dto)
    {
        if (!await KundeExistiert(kundeId)) return NotFound("Kunde nicht gefunden.");

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        var standort = new Standort { KundeId = kundeId, Name = dto.Name, Adresse = dto.Adresse };
        _db.Standorte.Add(standort);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details), new { kundeId, id = standort.Id }, standort);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Bearbeiten(int kundeId, int id, StandortDto dto)
    {
        var standort = await _db.Standorte.FirstOrDefaultAsync(s => s.Id == id && s.KundeId == kundeId);
        if (standort is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        standort.Name = dto.Name;
        standort.Adresse = dto.Adresse;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Loeschen(int kundeId, int id)
    {
        var standort = await _db.Standorte.FirstOrDefaultAsync(s => s.Id == id && s.KundeId == kundeId);
        if (standort is null) return NotFound();

        _db.Standorte.Remove(standort);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
