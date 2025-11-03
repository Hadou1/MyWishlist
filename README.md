# MyWishlist

MyWishlist ist eine produktionsreife .NET 9 Referenzimplementierung für eine mehrmandantenfähige Wunschlisten-Plattform mit vollständiger REST API, Persistenz, Authentifizierung, DSGVO-Funktionen und Docker-Unterstützung.

## Projektstruktur

```
MyWishlist.sln
├── Wishlist.Api            # ASP.NET Core Web API (Swagger, Auth, Fehlerbehandlung)
├── Wishlist.Application    # Use-Cases, Services, Validierung, Mapping
├── Wishlist.Contracts      # Request/Response DTOs
├── Wishlist.Domain         # Entities & Enums
├── Wishlist.Infrastructure # EF Core, DbContext, Migrations, Seed, JWT
└── Wishlist.Tests          # xUnit Tests (InMemory EF Core)
```

## Features

- REST API mit OpenAPI/Swagger (`/swagger`)
- JWT-basierte Authentifizierung (Register/Login, Rollen, Admin-Policy)
- Wunschlisten-ACL (Owner/Editor/Viewer), Link-Sharing mit optionalem Passcode, X-Robots-Tag für Link-Listen
- Artikel-Reservierungen und Käufe inkl. Maskierung bei fehlender Freigabe
- DSGVO-Funktionen (Datenexport, Löschung mit Anonymisierung)
- Einheitliches Fehlerformat `{ traceId, error { code, message, details } }`
- FluentValidation + AutoMapper im Applikationslayer
- EF Core + PostgreSQL, automatische Migrationen & Seed-Daten (Demo-User + Beispiel-Listen)
- Serilog Logging, Rate Limiting, CORS konfigurierbar
- Docker Compose Stack (API, PostgreSQL, Adminer)
- 12 automatisierte Tests (Validierung, ACL, Maskierung, Auth, Export)

## Lokales Setup

### Voraussetzungen

- .NET 9 SDK (Preview)
- Docker & Docker Compose

### Projekt bauen & Datenbank migrieren

```bash
dotnet restore
dotnet build
dotnet ef database update --project Wishlist.Infrastructure --startup-project Wishlist.Api
```

### Anwendung starten

```bash
dotnet run --project Wishlist.Api
```

Swagger UI steht unter `https://localhost:5001/swagger` (oder `http://localhost:5000/swagger`) bereit. Die API erwartet eine PostgreSQL-Verbindung via `DB_CONNECTION` oder `ConnectionStrings:Default`.

### Docker Compose

```bash
docker compose up --build
```

Services:
- API: http://localhost:8080
- PostgreSQL: localhost:5432
- Adminer: http://localhost:8081

Die API führt im Development-Modus automatische Migrationen und das Seed-Skript aus (Demo-Nutzer `demo@local` / `Passw0rd!`).

### Tests ausführen

```bash
dotnet test
```

## Konfiguration

Wichtige Umgebungsvariablen:

- `DB_CONNECTION` – PostgreSQL Connection String (z. B. `Host=localhost;Port=5432;Database=wishlist;Username=postgres;Password=postgres`)
- `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_KEY` – JWT-Konfiguration
- `ASPNETCORE_ENVIRONMENT` – beeinflusst CORS/Swagger/Migrationen

## DSGVO & Privacy

- `GET /api/v1/me/export` liefert ein JSON mit Benutzer-, Listen-, Artikel-, Reservierungs- und Kaufdaten
- `DELETE /api/v1/me` anonymisiert Benutzerkonto sowie referenzierte Reservierungen/Käufe
- Reservierungs-/Kauf-Identitäten bleiben für Besitzer verborgen, sofern `isVisibleToOwner=false`
- Link-basierte Listen senden `X-Robots-Tag: noindex`

## Seed-Daten

Bei Erststart (Development) werden automatisch erstellt:

- User `demo@local` (`Passw0rd!`) & `friend@local`
- Zwei Beispiel-Listen mit sechs Artikeln

## Wichtige Endpoints (Auszug)

- `POST /api/v1/auth/register`, `POST /api/v1/auth/login`
- `GET/DELETE /api/v1/me`, `GET /api/v1/me/export`
- `POST/GET/PATCH/DELETE /api/v1/lists`
- `GET /api/v1/lists/{id}?passcode=`, `POST /api/v1/lists/{id}/share-link`, `POST /api/v1/lists/{id}/share-passcode`
- `POST /api/v1/items`, `GET /api/v1/lists/{id}/items`
- `POST /api/v1/items/{id}/reservations`, `POST /api/v1/items/{id}/purchases`
- `POST /api/v1/lists/{id}/members`, `POST /api/v1/invites/lists/{id}`, `POST /api/v1/invites/accept`
- `GET /api/v1/admin/users`, `GET /api/v1/admin/lists`

## Lizenz

Dieses Projekt dient als Referenzimplementierung und kann als Ausgangsbasis für eigene Wishlist-Projekte genutzt werden.
