using System.ComponentModel.DataAnnotations;

namespace ITSupportVerwaltungsTool_Demo.Models;

public class Geraet
{
    public int Id { get; set; }

    [Required]
    public int KundeId { get; set; }
    public Kunde? Kunde { get; set; }

    [Required]
    public GeraeteTyp Typ { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Bezeichnung { get; set; }

    [MaxLength(50)]
    public string? Ip { get; set; }

    [MaxLength(100)]
    public string? Seriennummer { get; set; }
}
