using ITSupportVerwaltungsTool_Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSupportVerwaltungsTool_Demo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kunde> Kunden => Set<Kunde>();
    public DbSet<Geraet> Geraete => Set<Geraet>();
    public DbSet<Standort> Standorte => Set<Standort>();
    public DbSet<Benutzer> Benutzer => Set<Benutzer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Geraet>()
            .HasOne(g => g.Kunde)
            .WithMany(k => k.Geraete)
            .HasForeignKey(g => g.KundeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Standort>()
            .HasOne(s => s.Kunde)
            .WithMany(k => k.Standorte)
            .HasForeignKey(s => s.KundeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Benutzer>()
            .HasIndex(b => b.Benutzername)
            .IsUnique();

        modelBuilder.Entity<Geraet>()
            .Property(g => g.Typ)
            .HasConversion<string>();
    }
}
