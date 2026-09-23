# ZE — Portfolio Website

Production-ready full-stack portfolio for **ZE**, the web-dev studio founded by **Ziad Fayed** and **Mohamed Emad**.
Dark futuristic design (gold + electric blue, glassmorphism), content managed through an admin dashboard and stored in a database.

> **Stack:** ASP.NET Core 10 MVC · EF Core 10 · SQLite · ASP.NET Core Identity · Razor · CSS3/vanilla JS

---

## Features

**Public site**
- Hero (“Turning Ideas / Into Websites”), About, Skills, Featured Projects (max 4), Founders (“Two Minds, One Vision”), Services, Contact, Footer
- Project listing with search + pagination, project details at `/projects/{slug}`
- Contact form → stored in DB (email service is an injectable stub)
- SEO (title/meta/OG, semantic headings, alt text, favicon), responsive 320→1920px, no horizontal scroll
- Animations (scroll reveal, parallax, floating shapes, hover glow, scrollspy) with `prefers-reduced-motion` support

**Admin dashboard (`/Admin`)** — role-protected (`Admin`)
- Dashboard with stats, recent projects/messages
- CRUD for **Projects** (publish/featured toggles, slug auto-generation, cover-image upload, M:N technologies)
- CRUD for **Technologies**, **Skills**, **Services**, **Founders** (photo upload, accent color, order, visibility)
- **Messages**: search/filter (all/unread/read), pagination, mark read/unread, delete
- Search, pagination, confirm-delete modals, toast alerts, image previews

**Security**
- Identity with strong password + lockout policy, cookie auth, `[Authorize(Roles="Admin")]`
- Antiforgery tokens on every POST, input validation (server + client)
- Upload validation: extension + MIME + magic-byte check, ≤5 MB, unique filenames, stored only under `wwwroot/uploads/`
- Friendly 404/500 pages, no stack traces or DB errors exposed
- Admin credentials come from **user-secrets / environment variables only** — never committed

---

## Requirements

- [.NET SDK 10.x](https://dotnet.microsoft.com/download) (developed with 10.0.401)
- No database server needed — **SQLite** file database is created automatically (`ZE.db`)

## Getting started

```powershell
# 1. restore + build
dotnet restore .\ZE.csproj
dotnet build  .\ZE.csproj

# 2. (first run only) set the admin password — see below
dotnet user-secrets set "Seed:AdminPassword" "YourStrongPass1" --project .\ZE.csproj

# 3. run
dotnet run --project .\ZE.csproj
```

On startup the app:
1. applies EF Core migrations (creates `ZE.db` automatically),
2. seeds the `Admin` role, the admin user, founders (with the supplied photos), 14 skills, 6 services, 15 technologies and 6 sample projects.

- Site: `http://localhost:5144/` (see `Properties/launchSettings.json` for the configured ports)
- Admin: `http://localhost:5144/Admin`

### Admin credentials

| Setting  | Default             | Where                             |
|----------|---------------------|-----------------------------------|
| Email    | `admin@ze.local`    | `Seed:AdminEmail` (appsettings)   |
| Password | *(none — you set it)* | `Seed:AdminPassword` user-secret or `ZE_ADMIN_PASSWORD` env var |

If no password is configured, the admin account is skipped and a warning is logged.

```powershell
# option A — user secret (development, stored outside the repo)
dotnet user-secrets set "Seed:AdminPassword" "YourStrongPass1" --project .\ZE.csproj

# option B — environment variable (deployment)
$env:ZE_ADMIN_PASSWORD = "YourStrongPass1"
```

Password policy: min 8 chars, at least upper + lower case and a digit.

## Database

- **Provider:** SQLite (`Data Source=ZE.db` in `appsettings.json`) — zero-install, file-based.
- **Migrations:**

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --project .\ZE.csproj --output-dir Data/Migrations
dotnet ef database update --project .\ZE.csproj   # optional; the app migrates on startup
```

- Seeding logic lives in `Data/DbInitializer.cs` (idempotent — safe on every start).

### Switching to SQL Server later

1. Add `Microsoft.EntityFrameworkCore.SqlServer` and change `options.UseSqlite(...)` → `options.UseSqlServer(...)` in `Program.cs`.
2. Set `ConnectionStrings:DefaultConnection` to your SQL Server connection string.
3. Delete/regenerate migrations (`Data/Migrations`) and restart.

## Project structure

```
ZE/
├── Areas/Admin/            # protected dashboard (controllers, views, view components)
├── Controllers/            # public: Home, Projects, Contact
├── Data/                   # DbContext, DbInitializer (seed), Migrations
├── Models/                 # Project, Technology, ProjectTechnology, Skill, Service,
│                           # Founder, ContactMessage, ApplicationUser
├── Services/               # SlugHelper, ProjectService, ContentService,
│                           # LocalFileStorageService, IEmailService/NullEmailService
├── ViewModels/             # form + list view models (never expose EF entities directly)
├── Views/                  # public Razor views
├── wwwroot/
│   ├── css/site.css        # design system (tokens, glassmorphism, responsive)
│   ├── css/admin.css       # dashboard theme
│   ├── js/site.js          # navbar, reveal, parallax, particles, scrollspy
│   ├── js/admin.js         # sidebar, modals, previews, alerts
│   ├── images/             # ZE logo, islands, UI mockups, tech icons (extracted assets)
│   └── uploads/            # project covers + founder photos (runtime)
├── docs/references/        # original design reference images
└── tools/Extract-Assets.ps1# re-extracts assets from the reference sheets
```

## Design tokens

| Token        | Value                                          |
|--------------|------------------------------------------------|
| Backgrounds  | `#050A14` · `#07111F` · `#0A1628`               |
| Gold         | `#F5B942` · `#FFD36A`                          |
| Electric blue| `#168BFF` · `#38A7FF`                          |
| Text         | `#F5F7FA` (muted `#9AA8BC`)                    |
| Fonts        | Poppins (UI) · Yellowtail (script accents)     |

## Email

`IEmailService` is registered with `NullEmailService` (no-op). Contact messages are stored in the database.
Implement SMTP (e.g. MailKit) and register it in `Program.cs` to send real emails — no other changes required.
