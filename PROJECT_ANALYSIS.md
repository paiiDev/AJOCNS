# AJCONS — Complete Project Analysis

Full deep analysis of the AJCONS (PUPL Alumni &amp; Career Network) solution. Based on actual repository source code, the live SQL Server database (`AJOCNS_DB`), configuration, and all documentation files present in the repo. No files were modified during the analysis.

---

## 1. Project Architecture

### 1.1 Solution layout

Layered ("onion-style") .NET solution — `AJOCNS.sln`, four projects:

| Project | Layer | Responsibility | Key contents |
|---|---|---|---|
| `AJCONS.App` | Presentation (ASP.NET Core MVC, `net8.0`) | Controllers, Razor views, static assets, HTTP pipeline | `Controllers/*`, `Views/**`, `wwwroot/**`, `Program.cs`, `Models/ErrorViewModel.cs` |
| `AJOCNS.Domain` | Business logic | Services (business rules), background job | `Services/*` (8 services), `Interfaces/*`, `BackgroundJobs/EventStatusUpdateService.cs` |
| `AJOCNS.Database` | Data access | EF Core entities, DbContext, repositories | `Entities/*` (18), `Context/AppDbContext.cs` + `AppDbContext.Modifiers.cs`, `Repositories/*` (8), `Interfaces/*` (7), `Scripts/JobApplications.sql` |
| `AJOCNS.Shared` | Contracts | DTOs + shared helpers | `DTOs/**` (~44 classes), `Common/Result.cs`, `Common/MyanmarTime.cs` |

Dependency graph (applies strictly, one direction only):

```
AJCONS.App ──> AJOCNS.Domain ──> AJOCNS.Database
     │                │                └──> Microsoft.EntityFrameworkCore.SqlServer 8.0.30
     │                └──────> AJOCNS.Shared (Common, DTOs)
     └──────────────────────> AJOCNS.Shared
```

- **Repository pattern**: controllers never touch EF; services depend only on repository *interfaces* (`IStudentRepository`, `IEventRepository`, …).
- **`Result<T>` wrapper**: services never throw for expected failures; they return `Result<T>.Success(data)` / `Result<T>.Failure(message)`; controllers inspect `.IsSuccess` / `.ErrorMessage`.
- **Dependency injection** (all `AddScoped` in `Program.cs`): 7 repositories + 8 services + 1 hosted service.
- **Date handling**: all UTC stored in DB; all display/input converted through `MyanmarTime` (`Myanmar Standard Time` / `Asia/Yangon` fallback).

### 1.2 Request pipeline (`Program.cs`)

`AddControllersWithViews` → `AddDbContext<AppDbContext>` (`UseSqlServer(GetConnectionString("DBConnection"))`) → DI registrations → `AddHostedService<EventStatusUpdateService>` → cookie authentication (`LoginPath=/Auth/Login`, `LogoutPath=/Auth/Logout`, `AccessDeniedPath=/Auth/AccessDenied`) → `UseHttpsRedirection` → `UseStaticFiles` (x2) → routing → `UseAuthentication`/`UseAuthorization` → convention route `{controller=Home}/{action=Index}/{id?}`.

### 1.3 Background job

`EventStatusUpdateService` (BackgroundService): first runs immediately, then waits until next UTC midnight. Each cycle flips `Events` where `Status == "Upcoming"` and `EventDate < DateTime.UtcNow` to `"Completed"`.

---

## 2. Modules

1. **Authentication & Session** — login/logout, cookie principal, role-based redirect, remember-me (7-day expiry), first-login profile setup for students.
2. **Self-Registration** — Mentor registration (GRN + graduation-year verification), External Partner registration (company/position autocomplete), rejected-applicant re-submission.
3. **Admin User Management** — pending approval queue (mentor/partner), approve/reject with GRN/name/year verification, accreditation claim marking, activate/deactivate mentors, external-partner status toggle.
4. **Student Management (Admin)** — register student (SRN + temp password + welcome email), paged list, major/academic-year filters, bulk major update, bulk graduation update (transactional, auto GRN generation), edit (incl. academic year), soft-delete.
5. **Graduation Records (Admin)** — paged list, degree/year/keyword search, create (via bulk graduation), edit, delete (resets student status), degree options.
6. **Events** — create/edit/delete, poster upload, admin approval workflow (AutoApprove for admins), pending queue, status (Pending/Upcoming/Completed/Rejected), capacity-limited student registration, registrant listing, Zoom-link email blast.
7. **Job Posts** — create/edit/delete, approval workflow, open jobs listing, closing-date validation.
8. **Job Applications (Student)** — browse open jobs, apply with PDF resume + cover letter, applied-jobs history; partner side: applicant list, shortlist/reject with emails.
9. **Mentor Profile & Employment Records** — profile edit, employment history CRUD (get-or-create Company/Position), "current position" handling.
10. **External Partner Profile** — company/phone/expertise editing.
11. **Dashboards** — admin stats (active students/mentors, career events hosted, pending counts), student dashboard (profile + registered events + applied jobs), mentor/partner home.
12. **Background job** — auto-complete past events.

---

## 3. User Roles

Four roles, stored in `Users.Role` and enforced with `[Authorize(Roles = "...")]`:

| Role | Home redirect | Controllers authorized | Notes |
|---|---|---|---|
| **Admin** | `/Admin/Index` | `AdminController` (class-level `[Authorize(Roles="Admin")]`), plus `Event`/`Job` controllers; `ApproveEvent`/`RejectEvent`/`ApproveJobPost`/`RejectJobPost` actions `[Authorize(Roles="Admin")]` | Created via SQL seed (1 row in `Admins`). Registrar + content moderator. |
| **Student** | `/Student/Index` | `StudentController` (`"Student"`) | Self-service: events, jobs, profile |
| **Mentor** | `/Mentor/Index` | `MentorController` (`"Mentor"`) + `Event`/`Job` (as content creator) | Registration gate: pending → approved |
| **External Partner** | `/ExternalPartner/Index` | `ExternalPartnerController` (`"ExternalPartner"`) + `Event`/`Job` | Registration gate: pending → approved |

`HomeController.Index` and `AuthController.Login` perform role-based redirects. Unauthorized access → cookie middleware `AccessDeniedPath`.

---

## 4. Functional Requirements

### 4.1 Public / unauthenticated
- F1. Landing page (`/Home/Index`).
- F2. Login with email + password (BCrypt verify).
- F3. Register as Mentor (GRN, graduation year, name, expertise) — status `Pending`.
- F4. Register as External Partner (company, phone) — status `Pending`.
- F5. Access denied page.

### 4.2 Admin
- F10. Dashboard: counts of active students, active mentors, pending approvals, career events hosted.
- F11. Approve/reject pending mentor & partner accounts (mentor: verify GRN, graduation year, official name; grant accreditation).
- F12. Mentors list + details + activate/deactivate.
- F13. External partners list + status Active/Inactive.
- F14. Register new student (email, name, foundation major, programme, academic year) → SRN `PUPL-XXXXX`, temp password `PUPL@######`, welcome e-mail.
- F15. Student management: paged, filter by major & academic year, exclude-dropout, bulk major update, bulk graduation update, edit (name/phone/father/address/major/academic year/status), soft delete.
- F16. Graduation records: paged, filter by graduation year / degree / free search, edit (name, GRN, year, degree, accreditation status), delete.
- F17. Content approvals: pending events + job posts, approve/reject each.
- F18. Events / job board (as an organizer) incl. Create/Edit/Delete.

### 4.3 Student
- F30. First-login setup (phone, father name, address, new password) — mandatory once.
- F31. Dashboard with personal profile, reserved events, applied jobs.
- F32. Browse upcoming events; register (capacity-awareness, duplicate-block); view details incl. organizer mentor profile.
- F33. Job board (open jobs), job details, apply with PDF resume + cover letter; track application status.
- F34. Profile view.

### 4.4 Mentor
- F40. Dashboard with profile + employment-alert banner when no employment records.
- F41. Profile edit (name, expertise); employment history CRUD.
- F42. Manage own events/job posts (create/edit/delete; pending until admin approval); view registrants; send Zoom link.

### 4.5 External Partner
- F50. Dashboard, profile edit (name, phone, company, expertise).
- F51. Post events & jobs (pending until approval); my job posts; view applicants; shortlist/reject applicants (auto e-mail).

### 4.6 Cross-cutting
- F60. SweetAlert2 messages for all user feedback (success/error).
- F61. Role-aware navigation (sidebar, top navbar).
- F62. Automatic event completion (background service).

---

## 5. Non-Functional Requirements (as implemented)

- **Security**: BCrypt password hashing; anti-forgery tokens on all POSTs (`[ValidateAntiForgeryToken]`, `Html.AntiForgeryToken()`); role-based authorization; HTTPS redirection + HSTS in non-dev; HTML-encoding in e-mail bodies; server-side permission checks (owner-or-admin) on edit/delete of events/jobs and registrant views.
- **Integrity**: FK constraints + unique indexes at DB level (see §9–10); transactions for multi-row writes (bulk updates, user creation, student save).
- **Performance**: paged queries (`Skip`/`Take`, 10/page) for students, graduation records, events, jobs; `AsNoTracking` on read paths; indexes on FKs.
- **Maintainability**: 4-layer separation, interfaces, `Result<T>`, consistent DI.
- **Availability**: only `Home/Error` handler enabled in production; no retry/polly, no health checks.
- **Testing**: none (no test projects, no unit tests, no CI workflow).
- **Observability**: default `ILogger<HomeController>` only; no structured logging sinks.
- **Localization**: English-only UI with Myanmar font loaded; Myanmar-converted date/times.

---

## 6. Business Rules

### Authentication & accounts
1. Login blocked if `Status` is `Inactive` or `Pending`; error message "User not found, inactive, or pending approval."
2. Password verified with BCrypt; single canonical error "Invalid credentials."
3. Mentor/partner `Users.Status` lifecycle: `Pending` → `Active` (approved) or `Rejected` (with e-mail). Rejected users may re-submit with same email (only if role+profile shape match).
4. Students registered by admin are always `Active` + `IsFirstLogin=true`; first login forces profile/password setup.
5. Approval of a mentor requires: GRN exists in `Graduation_Records`, `GraduationYear` matches, and `OfficialName` equals provided name (case-insensitive). On success the record's `Acc_Status` is set to `Active` (accreditation claim) and mentor name is replaced by the record's official name.
6. Only `Active` mentors can be deactivated; only `Inactive` can be reactivated; rejected cannot be revived.

### Students
7. SRN format `PUPL-{n:D5}`, `n` = last numeric suffix + 1.
8. Default graduation status `Undergraduate`; a student is "Graduated" only if a `Graduation_Records` row exists (derived — free-text field is not trusted for display).
9. Registering a graduated student is not possible via the new-student form (foundation majors only).
10. Bulk graduation: setting `Graduated` creates a record only if none exists (degree from `Major.Degree_ID`, fallback to first degree, GRN sequential per year `PUPL-{year}-{n:D5}`); setting any other status deletes that student's records — transactional.
11. Edit-student guards a graduated student's status from changing.
12. Deleting a graduation record resets the student to `Undergraduate` when it was the last record.
13. Student delete is a soft delete: `Users.Status = Inactive` + `IsDeleted = true`.

### Events
14. Event date cannot be in the past (UTC, with 1h grace); admin can edit past events.
15. Non-admin-created events start `Pending`; admin-created start `Upcoming`; approval sets `Upcoming`; rejection sets `Rejected`.
16. Only `Upcoming` events accept registrations; duplicates rejected; capacity (if set) enforced by counting registrations.
17. Only the organizer (or admin) may view registrants, edit, delete, or send Zoom links.
18. Event registrations record `Status = Registered`, `RegistrationDate = UTC now`.

### Jobs
19. Closing date cannot be in the past; non-admin jobs start `Pending`; admin start `Open`; approval `Open`; rejection `Rejected`.
20. Applications require a PDF resume + cover letter; one application per (job, user) — DB unique index + repository guard.
21. Applying is rejected when job deleted, closing date passed, or status `rejected`.
22. Only the posting user (or admin) may manage a post.
23. Applicant statuses: `Pending` → `Shortlisted` / `Rejected`. Shortlisting auto-emails the student; rejecting auto-emails a polite decline.

### Shared
24. Created-by names resolve per role (Admin/Mentor/ExternalPartner tables → Name; otherwise Email).
25. All stored datetimes are UTC; all user-facing datetimes are Myanmar time via `MyanmarTime`.

---

## 7. Database Tables

Live database `AJOCNS_DB`. 18 application tables (+1 SQL Server system diagram table `sysdiagrams`).

| # | Table | Purpose |
|---|---|---|
| 1 | `Users` | Logins, roles, account status |
| 2 | `Admins` | Admin profile |
| 3 | `Students` | Student profile (SRN) |
| 4 | `Mentors` | Mentor profile + alumni credentials |
| 5 | `External_Partners` | Partner profile |
| 6 | `Majors` | Programmes of study |
| 7 | `Degrees` | Degrees offered |
| 8 | `Academic_Years` | Academic years |
| 9 | `Enrollments` | Student ↔ academic year |
| 10 | `Graduation_Records` | Certificates / GRN / accreditation |
| 11 | `Companies` | Employer companies |
| 12 | `Positions` | Job positions / roles |
| 13 | `EmploymentRecords` | Mentor employment history |
| 14 | `Events` | Career events |
| 15 | `Event_Types` | Event categories |
| 16 | `EventRegistrations` | Student event sign-ups |
| 17 | `JobPosts` | Job advertisements |
| 18 | `JobApplications` | Student applications + resumes |

Live reference data: 6 Degrees, 8 Majors, 28 Academic Years, 8 Event Types, 10 Companies, 4 Positions, 372 Graduation Records, 36 Users (12 students, 8 mentors, 9 partners, 1 admin), 13 Events, 9 JobPosts, 9 EventRegistrations, 6 JobApplications, 3 EmploymentRecords.

---

## 8. Attributes (columns)

`Users` — `User_ID` (PK), `Email` (unique), `PasswordHash`, `Role`, `Status`, `CreatedAt`, `isFirstLogin` (bit), `isDeleted` (bit) — all non-null except bit default.

`Admins` — `Admin_ID` (PK), `User_ID` (FK, unique), `Name`.

`Students` — `Student_ID` (PK), `User_ID` (FK, unique), `SRN` (unique), `Name`, `Phone` (null), `FatherName` (null), `Address` (null), `Major_ID` (FK), `GraduationStatus` (null).

`Mentors` — `Mentor_ID` (PK), `User_ID` (FK, unique), `Name`, `Expertise` (null), `Alumni_GY` (smallint), `Alumni_GRN`.

`External_Partners` — `External_Partner_ID` (PK), `User_ID` (FK, unique), `Name`, `Company_ID` (FK), `Phone` (null), `Expertise` (null), `Position_ID` (FK).

`Majors` — `Major_ID` (PK), `MajorName` (unique), `isFoundation` (bit, default 0, null), `Degree_ID` (FK, null).

`Degrees` — `Degree_ID` (PK), `DegreeName`, `DegreeCode`.

`Academic_Years` — `ACY_ID` (PK), `AcademicYear` (unique).

`Enrollments` — `ER_ID` (PK), `Student_ID` (FK), `ACY_ID` (FK), `Status` (default "Enrolled"), unique (Student_ID, ACY_ID).

`Graduation_Records` — `GRecord_Id` (PK), `OfficialName`, `GRN` (unique), `GraduationYear` (smallint), `Degree_ID` (FK), `Acc_Status` (default "Pending"), `Student_ID` (FK, null).

`Companies` — `Company_ID` (PK), `CompanyName` (unique).

`Positions` — `Position_ID` (PK), `Position` (unique).

`EmploymentRecords` — `Employment_R_ID` (PK), `Mentor_ID` (FK), `Company_ID` (FK), `Position_ID` (FK), `StartDate` (date), `EndDate` (date, null).

`Events` — `Event_ID` (PK), `CreatedByUser_Id` (FK), `EventTitle`, `Description` (null), `EventType_ID` (FK), `EventDate` (datetime2), `MaxCapacity` (int, null), `EventMode` (null), `Location` (null), `Status` (default "Upcoming"), `isDeleted` (bit), `posterImagePath` (null).

`Event_Types` — `EventType_ID` (PK), `EventTypeName` (unique).

`EventRegistrations` — `Event_Regi_ID` (PK), `Event_ID` (FK), `Student_ID` (FK), `Status` (default "Registered"), `RegistrationDate` (datetime2, default sysdatetime), unique (Student_ID, Event_ID).

`JobPosts` — `JobPost_Id` (PK), `Title`, `CompanyName`, `Description`, `Requirements` (null), `JobType` (null), `Location` (null), `SalaryRange` (null), `PostedDate` (datetime2, default getdate), `ClosingDate` (datetime2), `Status` (default "Pending"), `IsDeleted` (bit), `PostedByUserId` (FK).

`JobApplications` — `JobApplication_Id` (PK), `JobPostId` (FK), `User_Id` (FK), `CoverLetter`, `ResumeUrl` (null), `AppliedDate` (datetime2, default getutcdate), `Status` (default "Pending"), unique (JobPostId, User_Id).

---

## 9. Primary Keys & Foreign Keys

Primary keys (all identity `int`): `Users.User_ID`, `Admins.Admin_ID`, `Students.Student_ID`, `Mentors.Mentor_ID`, `External_Partners.External_Partner_ID`, `Majors.Major_ID`, `Degrees.Degree_ID`, `Academic_Years.ACY_ID`, `Enrollments.ER_ID`, `Graduation_Records.GRecord_Id`, `Companies.Company_ID`, `Positions.Position_ID`, `EmploymentRecords.Employment_R_ID`, `Events.Event_ID`, `Event_Types.EventType_ID`, `EventRegistrations.Event_Regi_ID`, `JobPosts.JobPost_Id`, `JobApplications.JobApplication_Id`.

Foreign keys (22 confirmed in `sys.foreign_keys`):

| Parent → Referenced | Column |
|---|---|
| `Admins → Users` | `User_ID` (1:1) |
| `Students → Users` | `User_ID` (1:1) |
| `Students → Majors` | `Major_ID` (N:1) |
| `Mentors → Users` | `User_ID` (1:1) |
| `External_Partners → Users` | `User_ID` (1:1) |
| `External_Partners → Companies` | `Company_ID` |
| `External_Partners → Positions` | `Position_ID` |
| `Majors → Degrees` | `Degree_ID` (nullable) |
| `Enrollments → Students` | `Student_ID` |
| `Enrollments → Academic_Years` | `ACY_ID` |
| `Graduation_Records → Degrees` | `Degree_ID` |
| `Graduation_Records → Students` | `Student_ID` (nullable) |
| `EmploymentRecords → Mentors` | `Mentor_ID` |
| `EmploymentRecords → Companies` | `Company_ID` |
| `EmploymentRecords → Positions` | `Position_ID` |
| `Events → Users` | `CreatedByUser_Id` |
| `Events → Event_Types` | `EventType_ID` |
| `EventRegistrations → Events` | `Event_ID` |
| `EventRegistrations → Students` | `Student_ID` |
| `JobPosts → Users` | `PostedByUserId` |
| `JobApplications → JobPosts` | `JobPostId` |
| `JobApplications → Users` | `User_Id` |

Additional DB constraints: unique indexes on `Email`, `SRN`, `GRN`, `CompanyName`, `Position`, `MajorName`, `EventTypeName`, `AcademicYear`, 1:1 `User_ID`s, and (Student,Event) / (Post,User) composite unique indexes.

---

## 10. Relationships & Cardinalities

- `User` 1:1 `Admin`, 1:1 `Student`, 1:1 `Mentor`, 1:1 `ExternalPartner` (four role profiles, exclusive in practice).
- `User` 1:N `Events` (as `CreatedByUser`), 1:N `JobPosts` (as `PostedByUser`), 1:N `JobApplications` (as applicant).
- `Student` 1:N `Enrollments`, N:1 `Major`; 1:N `GraduationRecords` (0..*), 1:N `EventRegistrations` (0..*, many-to-many to Event).
- `Major` N:1 `Degree` (nullable); `Degree` 1:N `Majors`, 1:N `GraduationRecords`.
- `AcademicYear` 1:N `Enrollments`.
- `Mentor` 1:N `EmploymentRecords`; `Company` 1:N `EmploymentRecords` + 1:N `ExternalPartners`; `Position` 1:N `EmploymentRecords` + 1:N `ExternalPartners`.
- `EventType` 1:N `Events`; `Event` 1:N `EventRegistrations`.
- `JobPost` 1:N `JobApplications`.
- Cardinality summary: 4 one-to-one, 13 one-to-many, 2 many-to-one-role-pair (User↔profiles enforced 1:1), 1 many-to-many via join table (`Students ↔ Events` through `EventRegistrations`).

---

## 11. CRUD Matrix

Legend: C=create, R=read, U=update, D=delete (soft-delete marked *); E=execute/action.

| Entity | Admin | Student | Mentor | External Partner |
|---|---|---|---|---|
| Users (auth) | RU (approve/reject/activate) | R | R | R |
| Students | CRUD* | R (self) | – | – |
| Mentors | RU | – | RU (profile) | – |
| External Partners | RU | – | – | RU (profile) |
| Majors | R | R | – | – |
| Degrees | R | – | – | – |
| Academic Years | R | R | – | – |
| Enrollments | CU | R | – | – |
| Graduation Records | CRUD | R (via student) | R (via claim) | – |
| Companies | R (via get-or-create on partner/mt) | – | RU (get-or-create) | RU (get-or-create) |
| Positions | R | – | RU (get-or-create) | R (get-or-create) |
| Employment Records | R (view mentor) | – | CRUD | – |
| Events | CRUD* + approve/reject | R + register (event action) | CRUD* (own) + view registrants + send link | CRUD* (own) + view registrants + send link |
| Event Types | R (only read via view) | R | R | R |
| Event Registrations | R | C (register) + R | R (own event) | R (own event) |
| JobPosts | CRUD* + approve/reject | R + apply (action) | CRUD* (own) | CRUD* (own) |
| Job Applications | – | C + RU status (view own) | – | RU status (applicants) |

Notes: students can only **apply** (create application) and view own; only the posting organizer or an admin can manage a post/event; only the organizer/admin can change application statuses of applicants on their own job.

---

## 12. Technologies

- **Runtime**: .NET 8 (ASP.NET Core MVC; `net8.0`).
- **ORM**: Entity Framework Core 8.0.30 (SqlServer, Design, Tools), code-first-style mapping but **DB was scaffolded** (no migrations history).
- **Database**: SQL Server (`AJOCNS_DB`), Trusted Connection.
- **Auth**: cookie authentication (`Microsoft.AspNetCore.Authentication.Cookies`), claims (NameIdentifier, Email, Name, Role), 7-day expiry.
- **Passwords**: BCrypt.Net-Next 4.2.0.
- **Frontend**: Bootstrap 5.3.3 (CDN), AdminLTE 3.2 (CDN, used selectively), FontAwesome 6.5.2 (CDN), jQuery 3.7.1 (CDN), jQuery Validate + Unobtrusive (CDN), SweetAlert2 11 (CDN), Google Fonts (Inter + Noto Sans Myanmar) — local copies exist for jQuery/Bootstrap validation bundles only (fallback).
- **Email**: `System.Net.Mail` SmtpClient, SMTP config section `EmailSettings` (SenderEmail, SenderName, SmtpServer, Port, AppPassword), SSL, HTML bodies.
- **Misc**: `IFormFile` uploads (event posters / resumes), `BackgroundService` hosted job, `TimeZoneInfo` Myanmar time.

---

## 13. Development Tools & Environment

- **IDE**: Visual Studio (ASPNET MVC template), .NET 8 SDK.
- **CLI**: `dotnet`; EF migration tooling installed as packages (Design/Tools) but unused.
- **Database tools**: SQL Server + `sqlcmd` (schema inspected via `sys.tables` / `INFORMATION_SCHEMA`).
- **Secrets**: ASP.NET User Secrets (`UserSecretsId 60a1f306-f4da-4408-96c7-4c12763777fd`) — connection string (`DBConnection`) and e-mail settings are NOT committed in `appsettings.json` (only Logging/AllowedHosts are).
- **VCS**: Git, single long-lived branch `master` (57 commits), `origin/master`; `.github/workflows` **empty** (no CI/CD); `.github/copilot-instructions.md` present.
- **Documentation**: `README.md` (1 line), `IMPLEMENTATION_REPORT.md` (beginner-facing feature/bug write-up).
- **No test projects, no .sql deployment scripts except `JobApplications.sql`.**

---

## 14. UI Pages

Public/auth (`Views`):
- `Home/Index`, `Home/Privacy` (stub), `Shared/Error`
- `Auth/Login`, `Auth/Register` (dual mentor/partner tabs), `Auth/AccessDenied`

Admin (11): `Index` (dashboard), `StudentManagement`, `RegisterNewStudent`, `EditStudent`, `GraduationRecords`, `EditGraduationRecord`, `UserApprovals`, `Mentors`, `MentorDetails`, `ExternalPartners`, `ExternalPartnerDetails`, `ContentApprovals` (pending posts).

Student (8): `Index` (dashboard), `Profile`, `FirstLoginSetup`, `Event`, `Job`, `JobBoard`, `JobDetails`, `_JobDetailsModal`, `_ApplyJobModal`. (**`CareerBuilder` view missing — action returns nonexistent view, see §15/16.**)

Mentor (5): `Index`, `Events`, `Jobs`, `Profile`, `_EmploymentRecordCard`.

External Partner (7): `Index`, `Events`, `Jobs` (redirects to MyJobPosts), `MyJobPosts`, `ViewApplicants`, `Profile`.

Event/Job shared (10): `Event/Index`, `Event/CreateEvent`, `Event/EditEvent`, `Event/Registrants`, `Event/SendZoomLink`, `Event/_EventDetailsModal`, `Job/Index`, `Job/CreateJobPost`, `Job/EditJobPost`, `Job/Details`.

Shared: `_Layout` (sidenav + topbar shell), `_Sidebar` (role-aware menu), `_ValidationScriptsPartial`.

Static: `wwwroot/css/site.css`, `wwwroot/js/site.js`, `wwwroot/favicon.ico`, `wwwroot/images/pupl.png` (logo) + `images/events/*` (uploaded posters), `wwwroot/uploads/resumes/*` (6 PDFs).

---

## 15. Implemented vs Incomplete Features

### Implemented (complete & wired)
- Login/logout cookies, role redirects, access-denied.
- Mentor & partner self-registration with approval emails; re-submission after rejection.
- Student registration (SRN, temp password, welcome email), first-login setup.
- Full student management (paging, filters, bulk major/graduation, edit, soft delete).
- Graduation records management (paging, year/degree/search, edit, delete, student-status sync, accreditation).
- Events: full lifecycle CRUD, poster upload, approval workflow, capacity, registration, registrants, Zoom-link emails.
- Jobs: full lifecycle CRUD, approval workflow, PDF-resume applications (unique per student), applicant statuses + emails.
- Mentor/partner profiles + employment-record CRUD with get-or-create company/position.
- Admin dashboards with stats; student dashboards.
- Background event auto-completion.
- Myanmar time conversion throughout; SweetAlert feedback; anti-forgery everywhere.

### Incomplete / gaps / dead paths
1. **No automated tests** and **no CI/CD** (`.github/workflows` empty). Regression protection is manual.
2. **`StudentController.CareerBuilder()`** returns `View()` but **`Views/Student/CareerBuilder.cshtml` does not exist** → any request to `/Student/CareerBuilder` is a 500. (No link reaches it; it is a dangling action.)
3. **Jobs never transition to `Closed`/`Completed`.** `Status` stays `Open` forever; closing only filters reads. The status vocabulary in code includes `Closed`/`Completed` (used in the admin `__published__` filter + dropdown) but nothing ever sets them.
4. **No password reset / forgot-password flow**; password can only be set on student first login or re-assignment by admin.
5. **No e-mail verification / OTP** for self-registration; trust is placed in the approval queue + GRN verification.
6. **No admin profile management**, no admin "last login"/audit log, no activity logging.
7. **Capacity is enforced in-app only** (count-before-insert); two concurrent registrations could exceed `MaxCapacity` (no DB-side check/lock).
8. **No pagination on `EventRegistrants`, `Job applicants`, `EmploymentRecords`, mentor lists**; some views load unbounded collections (e.g. `MyJobPosts` fetches 100, `GetAllEventsAsync` loads all).
9. **`Home/Privacy`** is default-template stub; not linked anywhere meaningful.
10. **README.md** is a placeholder (`# AJCONS`); only `IMPLEMENTATION_REPORT.md` documents the project, and it covers only student/graduation/admin flows (nothing on events, jobs, mentor/partner modules).
11. **Welcome e-mail contains placeholder login link** (`https://your-domain.com/login`) — not the real deployment URL.
12. **`Import`/`Export`**, online Zoom integration (link is manual), push notifications, and Myanmar-language UI are absent (font only).
13. Exception-resilience is minimal: repositories swallow exceptions and return `false` — root causes are often invisible (no logging in catches).
14. Dead code / vestigial pieces: `StudentRegistrationService.GenerateNewGRN()` (unused), `AppDbContext.OnConfiguring` hardcoded connection string (scaffold remnant, dead under DI since options are injected), empty `Migrations` folder, `Class1.cs` in Shared.

---

## 16. Inconsistencies Between Code, Database, and Documentation

### A. Documentation vs code/database
1. **Missing script promised by docs.** `IMPLEMENTATION_REPORT.md` §6 instructs running `AJOCNS.Database/Scripts/Alter_Majors_Add_Degree_ID.sql`. **That script does not exist** in the repo (only `Scripts/JobApplications.sql`). The schema change is already live in the DB, but the advertised deployment script is absent.
2. **Docs describe GRNs as sequential** (`PUPL-{year}-{n:D5}`) — correct for `StudentRepository.BulkUpdateGraduationsAsync`, but `RegisterStudent`'s dead helper `GenerateNewGRN()` still contains a random-suffix legacy version (`PUPL-{year}-{random 5}`) that would collide if ever used. Docs also don't mention that GRN sequence resets per graduation year.
3. **Docs' "Pending Approvals" understanding vs code.** Docs don't cover the dashboard stat, but `GetDashboardStatsAsync` counts pending *event registrations* via `Status.ToLower().Contains("pend")`. Event registrations are only ever written as `Registered`, so that term is **always 0** — the metric is effectively "pending events only". Code intent and data vocabulary don't match.
4. **Report claims "all fixtures wired in Program.cs"** — true for repos/services, but `IUserRepository`/`UserRepository` and `IEmailService` are also registered; fine. However the report's architecture table omits the **background job** and **Shared.Common** helpers entirely.

### B. Code vs database
5. **Hardcoded connection string in `AppDbContext.OnConfiguring`** (`Server=.;Database=AJOCNS_DB;Trusted_Connection=True;...`) coexists with DI-configured `GetConnectionString("DBConnection")` from user secrets. The `#warning` scaffold note is still there; the fallback would leak/lock a specific machine+DB and is inconsistent with config-driven setup.
6. **No `__EFMigrationsHistory`** table, yet EF Design/Tools packages + an **empty `Migrations` folder** exist. The database is effectively schema-first (scaffolded entity classes, `OnModelCreating` maps to existing tables). There is **no reproducible migration path** — deploying on a fresh machine requires the original DDL, which is not in the repo.
7. **Table/column naming chaos** (scaffold heritage): snake_case tables (`Graduation_Records`, `External_Partners`, `Academic_Years`, `Event_Types`) vs PascalCase tables (`EmploymentRecords`, `EventRegistrations`, `JobApplications`, `JobPosts`, `Events`, `Majors`, `Mentors`, `Students`, `Users`); lowercase columns (`isDeleted`, `isFirstLogin`, `posterImagePath`) vs Pascal (`CreatedByUser_Id`); camel columns (`PostedByUserId`, `JobPostId`, `User_Id`, `CoverLetter`) vs snake (`Major_ID`, `ACY_ID`, `Student_ID`). C# property names inherit this: `AcademicYear1`, `Position1`, `Major_ID`, `ACY_ID`, `Alumni_Gy`, `EmploymentRId`, `GrecordId`, `EventRegiId` — inconsistent with the rest of the codebase's PascalCase.
8. **Column length mismatches**: DB `EmploymentRecords.EndDate`/`StartDate` are `date` (no time) while the DTO/service treat them with `DateTime`/`TimeOnly.MinValue`, fine but the DB models store date-only; DB `Events.Description` = `nvarchar(MAX)` (`-1`) but entity HasMaxLength not set for Description; DTO `Description` limited to 1000/5000 chars in some forms while `JobPosts.Description` is `nvarchar(MAX)` in DB and `[StringLength(5000)]` in DTO — truncation risk above 5000 chars at edit time (over-post). Minor.
9. **Status vocabulary drift (jobs):** code writes `Pending`/`Open`/`Rejected`, but repository filters also honor `approved`, `active`, `published`, `closed`, `completed` (legacy vocabulary). Nothing sets the latter five, and the student `JobBoard` (`GetOpenJobPostsAsync`) **includes ExternalPartner jobs with status `Pending`** — an explicit exception clause — so students can see and apply to **unapproved** job posts, contradicting the create-flow message "It will be visible once approved by an admin". Events, by contrast, hide non-`Upcoming` events from students. Asymmetric, inconsistent approval semantics.
10. **Event capacity uniqueness**: DB unique index `UQ_Event_Registration_Student_Event` prevents duplicates, but `EventRegistrations.Status` is always `Registered` — the `Status` column is effectively dead (no cancel/withdrawn state), while repo `CountPendingEventRegistrationsAsync` probes `"pend"`.
11. **`EventStatusUpdateService` boundary `EventDate < DateTime.UtcNow`**: an event stored without timezone (UTC-converted) will complete strictly after its instant; combined with the 1-hour grace on creation, near-term events are fine. But if `EventDate` were ever stored local/unspecified (`MyanmarTime.ToUtc` is used consistently, so OK) — worth noting the correctness depends on all writers using UTC.
12. **`GetLastSRNAsync` orders by `Srn` string-wise** — correct only while zero-padded (PUPL-00001..00009...); at 6 digits ordering breaks. Format/limit not documented.
13. **Degree fallback in bulk graduation**: `BulkUpdateGraduationsAsync` falls back to "first degree by DegreeId" when a major has no linked degree — the exact bug class the docs claim was fixed by the `Major.Degree_ID` FK still has a silent fallback path in code.

### C. Code-internal inconsistencies
14. **`JobController.Details` vs `StudentController.JobDetails`** diverge: organizer/admin view any post (incl. pending/rejected); students view only "open" posts. Fine by role, but the same job-render path isn't reused.
15. **Redirect-after-create logic in `JobController.CreateJobPost`** checks `User.IsInRole("ExternalPartner")` for an Admin to route to `ExternalPartner/MyJobPosts` — a role check that is always false for admins, so it routes admins to `Job/Index`; convoluted but correct today.
16. **`HomeController` does not redirect `Mentor`** on home page (mentors fall through to the public landing view), while `AuthController.Login` does redirect mentors — subtle inconsistency in first-stop navigation (mentor lands on public home after login via remember-me only).
17. **`UserRepository.GetUserNameAsync` throws `ArgumentException` for unknown roles** — a future-facing role would 500 at login.
18. **Duplicate `app.UseStaticFiles()`** in `Program.cs` (lines 60 & 67).
19. **Version mix**: Domain project references `Microsoft.Extensions.Hosting.Abstractions` **10.0.11** on a `net8.0` target (unusual; the rest of the ecosystem is 8.x); Shared references `Microsoft.AspNetCore.Http.Features` **5.0.17** (pre-.NET8-era package). No centralized `Directory.Packages.props`, so dependency versions drift.
20. **SweetAlert temp data flow** is duplicated in every controller (no shared helper); the `_Layout` script renders messages from `TempData` inline — acceptable but repetitious and error-prone (e.g. `AuthController` uses `ViewData["SweetAlert_Error"]` while others use `TempData`).

### D. Documentation gaps
21. `README.md` is 1 line; no setup/build/run/seed instructions beyond `IMPLEMENTATION_REPORT.md`.
22. `IMPLEMENTATION_REPORT.md` does not cover the Events, Jobs, Mentor/Partner modules, the background service, or the DB tables that became scaffolded naming-mismatches — its "8 concepts" glossary also states soft-delete as *users* `IsDeleted=true`, which is only true for student deletes (events/jobs use their own `IsDeleted`).
23. No architecture decision record (ADR) explains why scaffolded snake_case was retained alongside hand-written PascalCase code.

---

*End of analysis. Generated from repo snapshot + live `AJOCNS_DB` schema/data at analysis time. Recommended next steps (not performed): decision on EF migrations adoption, add the missing `CareerBuilder` view or remove the action, CI + tests, and doc/code reconciliation.*