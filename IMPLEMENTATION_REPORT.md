# AJCONS — Implementation Report

A beginner-friendly explanation of every feature we built and every bug we fixed,
written against the actual code in this repository.

---

## Table of Contents

1. [Project Architecture](#1-project-architecture)
2. [Feature: Student Management](#2-feature-student-management)
3. [Feature: Manage Graduation](#3-feature-manage-graduation)
4. [Feature: Edit Student (with Academic Year)](#4-feature-edit-student-with-academic-year)
5. [Feature: Graduation Records Management](#5-feature-graduation-records-management)
6. [Database Design Change: Major → Degree Link](#6-database-design-change-major--degree-link)
7. [Bugs We Found and Fixed](#7-bugs-we-found-and-fixed)
8. [Concepts Glossary](#8-concepts-glossary)

---

## 1. Project Architecture

The solution is split into four projects (a **layered / onion architecture**). Each
layer only "talks" to the layer directly beneath it:

| Project | Responsibility | Example |
|---|---|---|
| `AJOCNS.Shared` | **DTOs** (Data Transfer Objects) — plain classes that carry data between layers and over HTTP | `StudentDto`, `PagedStudentDto`, `BulkGraduationUpdateRequestDto` |
| `AJOCNS.Database` | **Repositories** — all SQL / Entity Framework Core access lives here | `StudentRepository`, `GraduationRecordRepository` |
| `AJOCNS.Domain` | **Services** — business rules and validation; controllers never touch the database directly | `StudentRegistrationService`, `GraduationRecordService` |
| `AJOCNS.App` | **ASP.NET Core MVC app** — Controllers receive HTTP requests, Razor Views render HTML | `AdminController`, `Views/Admin/*.cshtml` |

Two patterns used everywhere:

- **Repository pattern**: an interface (`IStudentRepository`) describes *what* data
  operations exist; the repository class implements *how*. Services depend on the
  interface only.
- **`Result<T>` wrapper**: instead of throwing exceptions, services return
  `Result<bool>.Success(...)` or `Result<bool>.Failure("message")`. Controllers check
  `.IsSuccess` and show friendly error messages.

Everything is wired up in `Program.cs` with dependency injection
(`builder.Services.AddScoped<...>`), meaning one instance is created per web request.

---

## 2. Feature: Student Management

**Page:** `/Admin/StudentManagement` — `Views/Admin/StudentManagement.cshtml`

### What was implemented

- **Paged result list.** The database is never asked for all students at once.
  `GetStudentsPagedAsync(page, pageSize, majorId, acyId)` uses EF Core's
  `Skip()`/`Take()` to fetch one page (10 rows) plus a total count, wrapped in
  `PagedStudentDto`. Pagination links preserve the active filters.

- **Filter by Major and by Academic Year.** Two `<select>` dropdowns submit a GET
  form (`?majorId=...&acyId=...`). In the repository the query is filtered with
  `Where(s => s.Enrollments.Any(e => e.AcyId == acyId))` — a student belongs to an
  academic year through their **Enrollment** row.

- **Live search box.** Pure JavaScript: as you type, table rows whose text doesn't
  contain the search term are hidden. No server round-trip needed.

- **Academic Year column.** Each row shows the student's enrollment year
  (`Enrollments → Acy.AcademicYear1`).

### Bulk Major Update

Clicking **Bulk Major Update** toggles the UI: normal Edit/Delete buttons are hidden
and each row shows a major `<select>` instead. **Save All** collects
`{ StudentId, MajorId }` pairs and POSTs them as JSON to
`AdminController.BulkUpdateMajors`, which flows down to
`StudentRepository.BulkUpdateMajorsAsync` — all updates happen inside **one database
transaction**, so either everything saves or nothing does.

Key concepts used: anti-forgery tokens (`__RequestVerificationToken` header on
fetch calls), SweetAlert2 confirmations before destructive/batch actions, JSON
`[FromBody]` model binding.

---

## 3. Feature: Manage Graduation

**Entry point:** the *Manage Graduation* button beside *Bulk Major Update*.

### The flow

1. Clicking the button opens a **SweetAlert2 dialog with two inputs**:
   - *Academic Year (Enrollment Year)* — selects which students to load;
   - *Graduation Year* — typed **by hand**, so a delayed graduation can be recorded
     with its true year instead of "today's year".
2. Confirming redirects to `?acyId=X&gradMode=true&gradYear=Y`.
3. On page load, `enterGradMode()` runs automatically: action cells become
   Undergraduate/Graduated selects, and Save All becomes
   *"Save Graduation Statuses"*.
4. Saving POSTs `{ updates: [...], graduationYear: N }` to
   `AdminController.BulkUpdateGraduations`.

### What happens server-side

`StudentRepository.BulkUpdateGraduationsAsync(pairs, graduationYear)`, inside one
transaction, for each student:

- Sets `Students.GraduationStatus` to the chosen value.
- If set to **Graduated** and no record exists yet, creates a `GraduationRecord`
  automatically:
  - `DegreeId` comes from the student's **major's linked degree**
    (`student.Major.DegreeId`) — see [section 6](#6-database-design-change-major--degree-link);
  - `Grn` is generated sequentially per graduation year, e.g. `PUPL-2024-00001`;
  - `OfficialName` = student name, `AccStatus` = "Active".
- If set to anything else (**Undergraduate/Dropout**), any existing graduation
  records for that student are **deleted** — undoing a mistaken graduation also
  cleans up the record table.

---

## 4. Feature: Edit Student (with Academic Year)

**Page:** `/Admin/EditStudent/{id}`

- Added an **Academic Year (Enrollment Year)** dropdown. On save,
  `UpdateStudentEnrollmentAcyAsync` updates the student's latest Enrollment row —
  and if the student has **no enrollment yet, it creates one**
  (`Status = "Enrolled"`), so academic year can be assigned even to legacy records.
- The DTO carries `IsGraduated` (true when a graduation-record row actually exists).
  When graduated:
  - the **Programme Status field renders disabled** with a lock note;
  - a hidden input keeps the value valid for posting;
  - **server-side guard**: `UpdateStudentAsync` simply ignores the posted status for
    graduated students — even a forged request cannot un-graduate them. To change a
    graduate's status you must delete their graduation record first.
- Statuses supported: Undergraduate, Dropout (+ Graduated, managed via the
  graduation flow).

---

## 5. Feature: Graduation Records Management

**Page:** `/Admin/GraduationRecords` — mirrors Student Management:

| Capability | How it works |
|---|---|
| Paged result | Same Skip/Take pattern, `PagedGraduationRecordDto` |
| Filter by graduation year | Dropdown built from `GetDistinctGraduationYearsAsync()` (years that actually exist in records) |
| Live search | Same JavaScript row-hiding approach |
| Delete | Trash button + SweetAlert confirm → deletes the record **and resets the student's status** if it was their last record |
| Edit | Pencil button → `/Admin/EditGraduationRecord/{id}` |

**Edit graduation record** lets an admin correct Official Name, GRN, Graduation
Year, Degree (dropdown of all degrees), and Accreditation Status
(Active/Revoked). After saving, the service re-asserts that the linked student's
status stays `"Graduated"` so the two tables never disagree.

Navigation: a *Graduation Records* link sits in the Student Management header.

---

## 6. Database Design Change: Major → Degree Link

### The problem

`GraduationRecords` requires a `Degree_ID`, but the original schema had
**no relationship between Majors and Degrees**. Early code guessed the degree with
string matching, which produced wrong results (e.g. Civil Engineering students got
"Bachelor of Computer Science", the first row in Degrees).

### The fix — proper foreign key at the design level

New script: `AJOCNS.Database/Scripts/Alter_Majors_Add_Degree_ID.sql`

1. `ALTER TABLE Majors ADD Degree_ID INT NULL`
2. Backfill by known codes: Electronic Power → `EP`, Electronic → `EC`,
   Civil Engineering → `Civil`, Mechanical → `Mech`; fallback: match
   `DegreeName LIKE '%' + MajorName + '%'` (e.g. Computer Science → Bachelor of
   Computer Science).
3. Add constraint `FK_Majors_Degrees`.
4. **Repair existing bad records**: re-points any `GraduationRecord` whose degree
   differs from its student's current major's degree.

Entity/model changes: `Major.DegreeId` + `Major.Degree` navigation, mapped in
`AppDbContext` to column `Degree_ID`. Record creation now reads the degree straight
from the major — deterministic, no guessing.

> Run this script once against your database before deploying the new code.

---

## 7. Bugs We Found and Fixed

Each entry: symptom → root cause → fix.

### Bug 1 — False "Graduated" badge in Student Management
- **Symptom:** Students showed a green *Graduated* badge although the
  GraduationRecords table had no row for them.
- **Root cause:** The badge trusted the free-text `Students.GraduationStatus`
  string, which could say "Graduated" without any real record behind it.
- **Fix:** `ResolveGraduationStatus()` derives the displayed status from
  **actual data**: "Graduated" only if a record exists; a stale "Graduated" string
  displays as "Undergraduate".

### Bug 2 — Wrong degree on graduation records ("Civil Engineering" → CS degree)
- **Root cause:** No Major→Degree relationship (see section 6); code defaulted to
  the first degree.
- **Fix:** Schema-level `Majors.Degree_ID` FK + backfill + repair script; creation
  code uses `student.Major.DegreeId`.

### Bug 3 — Dropout students silently overwritten in Manage Graduation
- **Symptom:** After saving graduation statuses, students marked *Dropout*
  became *Undergraduate*.
- **Root cause:** The per-row select only offered Undergraduate/Graduated. A
  "Dropout" value matched neither option, so the browser fell back to the first
  option, and Save All posted that wrong value.
- **Fix:** When the current status isn't one of the two known values, it is added
  as a pre-selected third option — saving now preserves Dropout unless changed
  deliberately.

### Bug 4 — Cannot assign an Academic Year from Edit Student
- **Root cause:** `UpdateStudentEnrollmentAcyAsync` returned failure when the
  student had no Enrollment row, blocking the whole edit.
- **Fix:** Missing enrollment is now **created** instead of failing.

### Bug 5 — GRN stored the wrong year (and could collide)
- **Symptom:** Graduating someone as year 2024 in 2026 produced
  `PUPL-2026-xxxxx`; random suffixes could duplicate.
- **Fix:** Sequential GRNs derived from the *selected* graduation year
  (`PUPL-2024-00001`, `-00002`, …), computed from existing rows in the same
  transaction.

### Bug 6 — Stale "Graduated" status after deleting a record
- **Symptom:** Deleting a graduation record left `Students.GraduationStatus`
  = "Graduated" forever in the database (masked in lists, wrong in Edit Student).
- **Fix:** Deleting a record resets the linked student to "Undergraduate" when it
  was their last remaining record.

### Bug 7 — NaN posted when graduation-year missing
- **Symptom:** Opening `?gradMode=true` directly (bookmark/refresh) and clicking
  Save All sent `graduationYear: NaN` → confusing 400 error.
- **Fix:** Client-side guard explains that the year is missing and to re-enter it
  via Manage Graduation.

### Bug 8 — Accreditation badge styling never applied
- **Symptom:** No graduation record ever showed the green badge.
- **Root cause:** Compared `AccStatus == "Graduated"`, but the field stores
  Active/Revoked — dead code.
- **Fix:** Green badge for `Active`, red otherwise.

---

## 8. Concepts Glossary

- **DTO** — a plain class used to move data between layers/over HTTP, decoupling
  your API contract from database tables.
- **Repository pattern** — DB access isolated behind interfaces; services stay
  testable and storage-agnostic.
- **Paging (`Skip`/`Take`)** — fetch one window of rows at a time; essential once
  tables grow beyond hundreds of rows.
- **Transaction** — groups multiple writes so they all succeed or all roll back
  (used for bulk updates and record creation).
- **Foreign key (FK)** — a column enforcing that a value exists in another table
  (`Majors.Degree_ID → Degrees`). Fixes data-integrity bugs at the *design* level
  rather than in code.
- **Navigation property / `Include`** — EF Core loads related entities
  (`Include(s => s.Major)`) so you can traverse relationships in LINQ.
- **Anti-forgery token** — a hidden per-user token validated on POSTs, preventing
  cross-site request forgery.
- **Client-side vs server-side validation** — JS/SweetAlert gives instant feedback,
  but Data Annotations + controller checks are the real enforcement point (never
  trust the browser).
- **Soft delete** — users are deactivated (`IsDeleted = true`) rather than removed,
  preserving history.
