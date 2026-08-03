using System.Text;
using System.Text.Json.Serialization;
using ITSupportVerwaltungsTool_Demo.Data;
using ITSupportVerwaltungsTool_Demo.Models;
using ITSupportVerwaltungsTool_Demo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// test


// --- Datenbank (SQLite) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=itsupport.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// --- Services ---
builder.Services.AddScoped<TokenService>();

// --- Controller + Swagger ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums (z.B. GeraeteTyp) als Text statt als Zahl im JSON ausgeben
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JWT-Authentifizierung ---
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key fehlt in appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// --- CORS (nur relevant, falls Frontend separat aufgerufen wird) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("Standard", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// --- Datenbank anlegen/migrieren + Standard-Benutzer erzeugen ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Benutzer.Any())
    {
        var hasher = new PasswordHasher<object>();
        var standardBenutzer = new Benutzer
        {
            Benutzername = "admin",
            PasswortHash = hasher.HashPassword(new object(), "admin123")
        };
        db.Benutzer.Add(standardBenutzer);
        db.SaveChanges();
    }

    // --- Demo-Daten (nur beim allerersten Start, wenn noch keine Kunden vorhanden sind) ---
    if (!db.Kunden.Any())
    {
        var kundeMueller = new Kunde
        {
            Name = "Müller GmbH",
            Adresse = "Hauptstraße 12, 80331 München",
            Telefonnummer = "089 1234567",
            Email = "info@mueller-gmbh.de"
        };
        var kundeSchmidt = new Kunde
        {
            Name = "Schmidt & Partner",
            Adresse = "Bahnhofallee 5, 10115 Berlin",
            Telefonnummer = "030 9876543",
            Email = "kontakt@schmidt-partner.de"
        };

        db.Kunden.AddRange(kundeMueller, kundeSchmidt);
        db.SaveChanges();

        db.Geraete.AddRange(
            new Geraet { KundeId = kundeMueller.Id, Typ = GeraeteTyp.Server, Name = "DC-01", Bezeichnung = "Domaincontroller", Ip = "192.168.10.10", Seriennummer = "SN-0001" },
            new Geraet { KundeId = kundeMueller.Id, Typ = GeraeteTyp.Workstation, Name = "PC-Empfang", Bezeichnung = "Empfangs-PC", Ip = "192.168.10.51", Seriennummer = "SN-0002" },
            new Geraet { KundeId = kundeMueller.Id, Typ = GeraeteTyp.Drucker, Name = "Drucker-EG", Bezeichnung = "Etagendrucker Erdgeschoss", Ip = "192.168.10.80", Seriennummer = "SN-0003" },
            new Geraet { KundeId = kundeSchmidt.Id, Typ = GeraeteTyp.VPN, Name = "VPN-01", Bezeichnung = "Hauptstandort-VPN", Ip = "192.168.20.1", Seriennummer = "SN-0004" },
            new Geraet { KundeId = kundeSchmidt.Id, Typ = GeraeteTyp.Notebook, Name = "NB-GF", Bezeichnung = "Notebook Geschäftsführung", Ip = "192.168.20.30", Seriennummer = "SN-0005" }
        );

        db.Standorte.AddRange(
            new Standort { KundeId = kundeMueller.Id, Name = "Hauptsitz München", Adresse = "Hauptstraße 12, 80331 München" },
            new Standort { KundeId = kundeMueller.Id, Name = "Zweigstelle Augsburg", Adresse = "Fuggerstraße 3, 86150 Augsburg" },
            new Standort { KundeId = kundeSchmidt.Id, Name = "Büro Berlin", Adresse = "Bahnhofallee 5, 10115 Berlin" }
        );

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Standard");

// --- Frontend als statische Dateien ausliefern (gleicher Prozess) ---
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback: unbekannte Routen (außer /api/*) an index.html weiterreichen (SPA-Verhalten)
app.MapFallbackToFile("index.html");

app.Run();
