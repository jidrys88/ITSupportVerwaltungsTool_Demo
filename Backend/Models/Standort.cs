using System.ComponentModel.DataAnnotations;

namespace ITSupportVerwaltungsTool_Demo.Models;

public class Standort
{
    public int Id { get; set; }

    [Required]
    public int KundeId { get; set; }
    public Kunde? Kunde { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Adresse { get; set; }
}
