# AJCONS — PUPL Alumni & Career Network

AJOCNS (Alumni Job Opportunities & Career Networking System) is the official ASP.NET Core MVC web application for **PUPL — Polytechnic University (Panglong)**. It connects students, alumni mentors, and external hiring partners: managing student records and graduation, running a career-events calendar, and operating a job board with applications and an admin approval workflow.

---

## Table of Contents

- [Features](#features)
- [User Roles](#user-roles)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Database Setup](#database-setup)
  - [Build & Run](#build--run)
- [Database Scripts](#database-scripts)
- [Background Jobs](#background-jobs)
- [Business Rules (highlights)](#business-rules-highlights)
- [Documentation](#documentation)
- [Known Limitations](#known-limitations)

---

## Features

| Module | Summary |
|---|---|
| **Authentication & Session** | Cookie login/logout with BCrypt password hashing, role-based redirects, remember-me, mandatory first-login setup for students. |
| **Self-Registration** | Mentors register with GRN + graduation year; external partners register with company/position. Both enter a `Pending` state for admin approval. |
| **Admin User Management** | Approval queue for mentors/partners (verifies GRN/name/year), accreditation marking, mentor activate/deactivate, partner status toggle. |
| **Student Management** | Register student (auto SRN + temp password + welcome email), paged list, major/academic-year filters, live search, bulk major update, bulk graduation, edit, soft delete. |
| **Graduation Records** | Paged/filterable record list, create via bulk graduation, edit, delete (resets student status), degree + accreditation tracking. |
| **Events** | CRUD, poster upload, admin approval workflow, capacity-limited student registration, registrant listing, Zoom-link email blast, auto-completion of past events. |
| **Job Board** | CRUD, approval workflow, open-jobs listing, PDF-resume applications, applicant shortlist/reject with automatic emails, auto-closing of expired posts. |
| **Mentor & Partner Profiles** | Profile editing, mentor employment-history CRUD with get-or-create companies/positions, partner company/expertise editing. |
| **Dashboards** | Admin statistics (students, mentors, events, pending counts), student dashboard (profile, events, applications), mentor/partner home pages. |

---

## User Roles

Four roles are stored in `Users.Role` and enforced with `[Authorize(Roles = "...")]`.

| Role | Landing Page | Capabilities |
|---|---|---|
| **Admin** | `/Admin/Index` | Registrar + content moderator: student/graduation management, approvals, events, jobs. |
| **Student** | `/Student/Index` | Browse/register events, browse/apply jobs, manage own profile. |
| **Mentor** | `/Mentor/Index` | Content creator (events/jobs) + employment profile; requires admin approval. |
| **External Partner** | `/ExternalPartner/Index` | Post events/jobs, manage applicants; requires admin approval. |

Admin accounts are seeded directly in the database (there is no public admin registration).

---

## Architecture

Layered ("onion-style") solution with a strict one-directional dependency flow:

```
AJCONS.App  ──>  AJOCNS.Domain  ──>  AJOCNS.Database  ──>  SQL Server
     │                  │                    │
     └──────────────> AJOCNS.Shared <────────┘
              (DTOs, Result<T>, MyanmarTime)
```

| Project | Layer | Responsibility |
|---|---|---|
| `AJCONS.App` | Presentation | ASP.NET Core MVC — controllers, Razor views, static assets, DI wiring (`Program.cs`). |
| `AJOCNS.Domain` | Business logic | Services (business rules), email, background jobs. |
| `AJOCNS.Database` | Data access | EF Core entities, `AppDbContext`, repositories, SQL scripts. |
| `AJOCNS.Shared` | Contracts | DTOs plus shared helpers (`Result<T>`, `MyanmarTime`). |

Key patterns:

- **Repository pattern** — controllers never touch EF Core directly; services depend on repository interfaces only.
- **`Result<T>` wrapper** — services return `Result<T>.Success(...)` / `Result<T>.Failure(...)` for expected failures instead of throwing; controllers inspect `.IsSuccess` / `.ErrorMessage`.
- **Dependency injection** — repositories, services, and hosted background services are registered as scoped/singleton in `Program.cs`.
- **UTC storage / Myanmar display** — all datetimes are stored in UTC and converted for display via `MyanmarTime`.

---

## Technology Stack

- **Runtime:** .NET 8 (`net8.0`), ASP.NET Core MVC
- **ORM:** Entity Framework Core 8.0.30 (SQL Server provider)
- **Database:** Microsoft SQL Server (`AJOCNS_DB`, Trusted Connection)
- **Authentication:** Cookie authentication (claims: NameIdentifier, Email, Name, Role)
- **Passwords:** BCrypt.Net-Next 4.2.0
- **Frontend (CDN):** Bootstrap 5.3.3, AdminLTE 3.2, FontAwesome 6.5.2, jQuery 3.7.1, jQuery Validate/Unobtrusive, SweetAlert2 11, Google Fonts (Inter, Poppins, Noto Sans Myanmar)
- **Email:** `System.Net.Mail` (`SmtpClient`, SSL, HTML bodies)
- **Misc:** `IFormFile` uploads (event posters, PDF resumes), `BackgroundService` hosted jobs, `TimeZoneInfo` Myanmar time

---

## Project Structure

```
AJOCNS.sln
├─ AJCONS.App/                     # ASP.NET Core MVC presentation
│  ├─ Controllers/                 # Admin, Auth, Student, Mentor, ExternalPartner, Event, Job, Home
│  ├─ Views/                       # Razor views per controller + Shared layout
│  ├─ wwwroot/                     # css, js, images, uploads
│  ├─ Program.cs                   # DI + HTTP pipeline
│  └─ appsettings.json
├─ AJOCNS.Domain/                  # Business logic
│  ├─ Services/                    # Auth, Email, Student, Event, Job, Mentor, GraduationRecord, ...
│  ├─ Interfaces/
│  └─ BackgroundJobs/              # EventStatusUpdateService, JobStatusUpdateService
├─ AJOCNS.Database/                # Data access
│  ├─ Context/                     # AppDbContext (+ Modifiers partial)
│  ├─ Entities/                    # 18 EF Core entities
│  ├─ Repositories/                # 7 repositories
│  ├─ Interfaces/
│  └─ Scripts/                     # SQL scripts
└─ AJOCNS.Shared/                  # Contracts
   ├─ DTOs/                        # Auth, Events, Jobs, Student, Mentor, GraduationRecords, ...
   └─ Common/                      # Result.cs, MyanmarTime.cs
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (local/LocalDB/Express) with an `AJOCNS_DB` database
- An SMTP account for outbound email (optional for local runs — email calls will fail gracefully)
- Visual Studio 2022 or VS Code (optional)

### Configuration

Sensitive settings are **not** committed. Provide them via **ASP.NET User Secrets** (the app has `UserSecretsId` `60a1f306-f4da-4408-96c7-4c12763777fd`).

From `AJCONS.App`:

```powershell
dotnet user-secrets set "ConnectionStrings:DBConnection" "Server=.;Database=AJOCNS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "EmailSettings:SenderEmail" "you@example.com"
dotnet user-secrets set "EmailSettings:SenderName" "AJCONS"
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.example.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:AppPassword" "your-app-password"
```

Alternatively, add a `ConnectionStrings:DBConnection` entry (and an `EmailSettings` section) to `AJCONS.App/appsettings.Development.json`.

> A hardcoded fallback connection string also exists in `AppDbContext.OnConfiguring`; DI-provided configuration takes precedence, but it should be removed for production.

### Database Setup

The project uses a **schema-first** approach (no EF migration history). The database DDL is not fully contained in the repo, so a fresh environment must be provisioned from the existing `AJOCNS_DB` schema, then run the incremental scripts in order:

1. `AJOCNS.Database/Scripts/JobApplications.sql`
2. `AJOCNS.Database/Scripts/Alter_Majors_Add_Degree_ID.sql`
3. `AJOCNS.Database/Scripts/Add_Academic_Years_Through_2035.sql`

All scripts are idempotent (they check for existing objects before altering).

### Build & Run

```powershell
# Restore + build
dotnet restore AJOCNS.sln
dotnet build AJOCNS.sln

# Run the web app (defaults to https://localhost:xxxx)
dotnet run --project AJCONS.App
```

The app redirects unauthenticated users to `/Auth/Login`. Admin accounts must be created directly in the `Users` + `Admins` tables.

---

## Database Scripts

| Script | Purpose |
|---|---|
| `JobApplications.sql` | Creates/fixes the `JobApplications` table and its constraints. |
| `Alter_Majors_Add_Degree_ID.sql` | Adds `Majors.Degree_ID` FK to `Degrees`, backfills by degree code, and repairs incorrect graduation-record degrees. |
| `Add_Academic_Years_Through_2035.sql` | Seeds academic-year reference rows. |

---

## Background Jobs

Both jobs run at startup and then once per day (at UTC midnight):

- **`EventStatusUpdateService`** — flips `Upcoming` events whose `EventDate` has passed to `Completed`.
- **`JobStatusUpdateService`** — flips `Open` job posts whose `ClosingDate` has passed to `Closed`.

---

## Business Rules (highlights)

- **Account lifecycle:** mentor/partner accounts go `Pending → Active` or `Rejected`; rejected users may re-submit with the same email.
- **Mentor approval** requires the GRN to exist in `Graduation_Records`, the graduation year to match, and the official name to match; on success the record is marked accredited.
- **Students:** SRN format `PUPL-{n:D5}`; a student is "Graduated" only when a graduation record exists (derived, not trusted from free text).
- **Bulk graduation:** generates sequential GRNs per graduation year (`PUPL-{year}-{n:D5}`); reverting a status removes the record — all inside a transaction.
- **Events:** dates cannot be in the past (1h grace); non-admin events start `Pending`; only `Upcoming` events accept registrations with capacity enforcement; only the organizer or an admin can manage registrants/send Zoom links.
- **Jobs:** closing dates cannot be past; non-admin posts start `Pending`; one application per (job, student); applications require a PDF resume + cover letter; shortlist/reject trigger automatic emails.
- **Date handling:** all datetimes stored UTC; all user-facing values converted to Myanmar time.

---

## Documentation

- [`PROJECT_ANALYSIS.md`](PROJECT_ANALYSIS.md) — full architecture, database schema, CRUD matrix, requirements, and a detailed gap/inconsistency analysis.
- [`IMPLEMENTATION_REPORT.md`](IMPLEMENTATION_REPORT.md) — beginner-friendly feature/bug write-up for the student and graduation modules.

---

## Known Limitations

- No automated tests and no CI/CD pipeline.
- No password reset / forgot-password flow.
- `StudentController.CareerBuilder()` action exists without a matching view.
- Capacity checks are application-side only (not concurrency-safe).
- Several list views (registrants, applicants, employment records) are not paginated.
- Connection string fallback and scaffolded naming inconsistencies remain in the data layer.
