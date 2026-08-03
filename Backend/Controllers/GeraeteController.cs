using ITSupportVerwaltungsTool_Demo.Data;
using ITSupportVerwaltungsTool_Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSupportVerwaltungsTool_Demo.Controllers;

public record GeraetDto(GeraeteTyp Typ, string Name, string? Bezeichnung, string? Ip, string? Seriennummer);

[ApiController]
[Authorize]
[Route("api/kunden/{kundeId}/geraete")]
public class GeraeteController : ControllerBase
{
    private readonly AppDbContext _db;

    public GeraeteController(AppDbContext db)
    {
        _db = db;
    }

    private async Task<bool> KundeExistiert(int kundeId) =>
        await _db.Kunden.AnyAsync(k => k.Id == kundeId);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Geraet>>> Liste(int kundeId, [FromQuery] GeraeteTyp? typ)
    {
        if (!await KundeExistiert(kundeId)) return NotFound("Kunde nicht gefunden.");

        var query = _db.Geraete.Where(g => g.KundeId == kundeId);
        if (typ.HasValue)
        {
            query = query.Where(g => g.Typ == typ.Value);
        }

        var geraete = await query.OrderBy(g => g.Name).ToListAsync();
        return Ok(geraete);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Geraet>> Details(int kundeId, int id)
    {
        var geraet = await _db.Geraete.FirstOrDefaultAsync(g => g.Id == id && g.KundeId == kundeId);
        if (geraet is null) return NotFound();
        return Ok(geraet);
    }

    [HttpPost]
    public async Task<ActionResult<Geraet>> Erstellen(int kundeId, GeraetDto dto)
    {
        if (!await KundeExistiert(kundeId)) return NotFound("Kunde nicht gefunden.");

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        var geraet = new Geraet
        {
            KundeId = kundeId,
            Typ = dto.Typ,
            Name = dto.Name,
            Bezeichnung = dto.Bezeichnung,
            Ip = dto.Ip,
            Seriennummer = dto.Seriennummer
        };

        _db.Geraete.Add(geraet);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details), new { kundeId, id = geraet.Id }, geraet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Bearbeiten(int kundeId, int id, GeraetDto dto)
    {
        var geraet = await _db.Geraete.FirstOrDefaultAsync(g => g.Id == id && g.KundeId == kundeId);
        if (geraet is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name wird benötigt.");
        }

        geraet.Typ = dto.Typ;
        geraet.Name = dto.Name;
        geraet.Bezeichnung = dto.Bezeichnung;
        geraet.Ip = dto.Ip;
        geraet.Seriennummer = dto.Seriennummer;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Loeschen(int kundeId, int id)
    {
        var geraet = await _db.Geraete.FirstOrDefaultAsync(g => g.Id == id && g.KundeId == kundeId);
        if (geraet is null) return NotFound();

        _db.Geraete.Remove(geraet);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
