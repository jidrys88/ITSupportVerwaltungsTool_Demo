using System.ComponentModel.DataAnnotations;

namespace ITSupportVerwaltungsTool_Demo.Models;

public class Kunde
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Adresse { get; set; }

    [MaxLength(50)]
    public string? Telefonnummer { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public ICollection<Geraet> Geraete { get; set; } = new List<Geraet>();
}
