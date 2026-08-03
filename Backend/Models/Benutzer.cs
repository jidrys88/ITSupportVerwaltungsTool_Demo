using System.ComponentModel.DataAnnotations;

namespace ITSupportVerwaltungsTool_Demo.Models;

public class Benutzer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Benutzername { get; set; } = string.Empty;

    [Required]
    public string PasswortHash { get; set; } = string.Empty;
}
