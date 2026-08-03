# IT Support Verwaltungstool (vereinfachte Version)

Minimalistische Version: nur Kundenverwaltung und Geräteverwaltung (ohne
Zugangsdaten, ohne Standorte, ohne Etikettendruck, ohne 2FA, ohne Rollen).

- **Backend:** .NET 8 Web API (C#), SQLite
- **Frontend:** reines HTML/CSS/JavaScript, liegt in `Backend/wwwroot` und wird
  vom selben Prozess mitausgeliefert
- **Login:** einfacher Benutzername/Passwort-Login, keine Rollen, keine 2FA
  (Standard-Zugang beim ersten Start: `admin` / `admin123` – bitte danach
  in der Datenbank ändern)

## Starten

```bash
cd Backend
dotnet restore
dotnet run
```

Die App ist danach erreichbar unter `http://localhost:5000` (Frontend +
API im selben Prozess). Die SQLite-Datei `itsupport.db` wird beim ersten
Start automatisch angelegt.

Beim allerersten Start werden zusätzlich zwei Demo-Kunden mit ein paar
Beispielgeräten angelegt, damit man die Anwendung sofort ausprobieren kann.

## Wichtig vor produktivem Einsatz

- `Jwt:Key` in `Backend/appsettings.json` durch einen eigenen,
  zufälligen, mindestens 32 Zeichen langen Wert ersetzen
- Standard-Admin-Passwort ändern
- HTTPS aktivieren, sobald das System außerhalb des eigenen Rechners
  erreichbar ist

## Struktur

```
ITSupportVerwaltungsTool_Demo/
├── ITSupportVerwaltungsTool_Demo.sln
└── Backend/
    ├── Backend.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Models/        (Kunde, Geraet, GeraeteTyp, Benutzer)
    ├── Data/           (AppDbContext – SQLite)
    ├── Controllers/    (AuthController, KundenController, GeraeteController)
    ├── Services/       (TokenService – JWT)
    └── wwwroot/        (login.html, index.html, css/, js/)
```

## Funktionsumfang

- Kunden anlegen, bearbeiten, löschen, suchen
- Geräte pro Kunde anlegen, bearbeiten, löschen (10 Gerätetypen)
- Login (Benutzername + Passwort), kein 2FA, keine Benutzerrollen
- Hauptmenü oben: nur "Startseite" und "Abmelden"

## Mögliche Erweiterungen (Zukunft)

Diese Version ist bewusst minimal. Mögliche spätere Ausbaustufen:

- **Zugangsdaten pro Gerät**: AES-verschlüsselte Logins/Passwörter je Gerät
- **Standorte**: mehrere Standorte/Filialen pro Kunde
- **Benutzerrollen**: Admin/Techniker mit unterschiedlichen Rechten
- **Zwei-Faktor-Authentifizierung (TOTP)**: zusätzliche Login-Absicherung
- **Soft-Delete & Wiederherstellen**: gelöschte Kunden/Geräte reaktivierbar
- **Audit-Log**: Protokoll aller Änderungen (wer, wann, was)
- **Etikettendruck**: QR-Code + Barcode pro Gerät für Etikettendrucker
- **Seriennummern-Produktion**: mehrere baugleiche Geräte in Serie anlegen
- **IP-Übersicht**: alle IP-Adressen eines Kunden auf einen Blick
- **Automatische Abmeldung** nach Inaktivität
- **PWA-Installierbarkeit** für mobile Nutzung beim Kunden vor Ort
- **PostgreSQL statt SQLite** für den Mehrbenutzer-/Produktivbetrieb
