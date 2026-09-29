# Healthcarev2 / OmniSync

Integrated React + TypeScript application backed by ASP.NET Core 10, EF Core and PostgreSQL. The production UI uses real APIs. There are no seeded accounts, patients, appointments, or browser-only business records in the application.

## Repository

- `backend/`: existing models, snake_case EF mappings, migrations, DTOs, controllers and services.
- `backend/Security/`: request identity, SQL resource scopes and API authorization/transaction filter.
- `backend/Services/Availability/`: shared scheduling calculation used by discovery and booking.
- `backend/Services/Reports/`: server-side activity and recorded-collection aggregates.
- `backend.Tests/`: xUnit HTTP integration tests with real JWT authentication and isolated PostgreSQL databases.
- `frontend/src/`: typed API client, authentication, routing, shared forms, calendar and application pages.
- `frontend/e2e/`: Playwright workflows against the real API and database.
- `verification/`: starts an isolated full-stack test environment and removes it after verification.
- `docs/phase-2-report.md`: implementation/verification report and remaining limits.

The original controller → service → EF Core architecture and all 28 entities remain. No Phase 2 schema changes or migrations were needed. Doctor IDs remain shared user IDs; appointments still support multiple visits; files retain their polymorphic parent reference.

## Local prerequisites

- .NET 10 SDK.
- Node.js compatible with the locked Vite version (this implementation was checked with Node 24).
- Local PostgreSQL (checked with PostgreSQL 17).
- An existing application database/account configuration. Tests require a PostgreSQL role with permission to create and drop **separate test databases**.

## Backend configuration and startup

Supply development settings in ignored `backend/appsettings.Development.json`, or use environment variables:

```text
ConnectionStrings__DefaultConnection=your-local-PostgreSQL-connection
Jwt__Key=your-secret-signing-key-at-least-32-bytes
Jwt__Issuer=Healthcare.Api
Jwt__Audience=Healthcare.Client
Jwt__ExpirationMinutes=60
Healthcare__TimeZone=Asia/Beirut
```

Do not commit real credentials or signing keys. Startup validates the required settings. An existing administrator account is needed to provision users; there is no public registration or default production password.

```powershell
dotnet restore Healthcarev2.sln
dotnet build Healthcarev2.sln --no-restore
dotnet run --project backend/Healthcare.Api.csproj --launch-profile http
```

The existing HTTPS profile uses `https://localhost:7198`; the HTTP profile uses `http://localhost:5252`. Trust your local ASP.NET development certificate as appropriate for your machine.

Inspect the existing migrations before applying them to any database. The doctor shared-key migration assumes matching user IDs already exist; existing staff accounts must have a valid assignment. No migration is applied automatically at application startup. Back up and validate existing data before operator-managed migration deployment.

Development OpenAPI is available to authenticated users at `/openapi/v1.json`. `/health` is a minimal anonymous database connectivity check; it does not expose records or configuration.

## Frontend setup

```powershell
cd frontend
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. The Vite development proxy defaults to the normal backend HTTP profile at `http://localhost:5252`; no override is needed. To use the HTTPS profile instead:

```powershell
$env:API_PROXY_TARGET = 'https://localhost:7198'
npm run dev
```

`VITE_API_BASE_URL` configures the browser API base URL at build time; its default is `/api`. See `frontend/.env.example`. Development proxy certificate verification is disabled only for the local development certificate. Production should use a trusted HTTPS endpoint and same-origin API routing, or deliberately configured CORS at the application boundary.

## Authentication and authorization

Login uses `POST /api/auth/login`. The JWT is centrally held in per-tab `sessionStorage`, restored via `GET /api/auth/me`, attached by `api/client.ts`, and cleared on logout or an invalid session. This is bearer-token auth, not cookie/refresh-token auth. Logout does not revoke a copied token server-side. No refresh/revocation system was introduced.

The existing development JWT lifetime remains **60 minutes**, with zero server clock skew. The UI warns during the final two minutes, checks known expiry before submitting requests, and explains expiry/unsaved work on the login screen. It does not extend token lifetime, refresh tokens, or persist clinical form drafts. Production lifetime remains controlled by `Jwt:ExpirationMinutes`.

Every authenticated request checks the current active user and role in PostgreSQL. A changed role invalidates the old token. Staff assignments are loaded from the database on each request; staff must have exactly one assigned doctor or organization.

| Role | Scope and writes |
|---|---|
| Admin | Administrative and clinical access; hard deletion where existing restrictive foreign keys allow it. Audit API is read-only. |
| Doctor | Own appointments and clinical records; demographic reads only; own schedules/exceptions. Cannot book, reschedule, cancel appointments, or hard-delete clinical history. |
| Doctor secretary | Assigned doctor's operational resources and relevant patients. |
| Organization secretary | Doctors whose organization matches the assignment; organization locations and relevant patients. |
| Anonymous | Login, minimal health check and availability times only. |

Secretaries may create/read/update visits and payments and create/update relevant patient demographics. They may read scoped consultations, prescriptions/items, follow-ups, lab/imaging requests and their permitted attachments, but cannot author clinical records. The consultation-read rule was explicitly approved during implementation.

Patient visibility is based on a **non-cancelled appointment OR any historical visit** involving an authorized doctor. A cancelled appointment alone does not establish access. A shared patient does not grant access to another doctor's clinical records.

New patients may temporarily have no relationship. The booking form retains a newly created patient's returned ID and immediately supports booking; that appointment establishes normal visibility. Global patient lookup is intentionally unavailable to staff. If a globally unique MRN already exists outside a secretary's scope, an administrator must resolve registration rather than exposing unrelated demographics.

`ResourceAccess.Query<T>` applies relationship scopes in SQL. The API filter checks existing resources and requested parent targets. Out-of-scope individual records normally return 404; forbidden operations return 403. Frontend role guards supplement, and never replace, backend checks.

## Scheduling and appointment lifecycle

- All working hours and exception wall times use **Asia/Beirut**. Weekdays in the API are Sunday=0 through Saturday=6.
- `VisitType.DefaultDuration` determines the appointment duration. Client-supplied end times are ignored; create/update DTOs no longer expose them.
- `Schedule.SlotDuration` determines the interval between valid **start times**, anchored at the schedule start. It does not need to equal visit duration.
- Active doctor, location, DoctorLocation, visit type, and applicable working time are required for new bookings/reschedules.
- Unavailable exceptions subtract time; available exceptions **add** working intervals, including days without recurring hours.
- Extra-hours start intervals use the smallest active interval for that mapping on the weekday, otherwise the smallest active interval on the mapping, otherwise the selected visit duration. Starts anchor at the exception start. This deterministic fallback avoids inventing a database column.
- Overlapping working intervals are merged for coverage, while start anchors remain explicit. Duplicate candidates are removed.
- Busy appointments block the doctor across **all locations**. Cancelled appointments do not block; touching endpoints are permitted.
- Duplicate active DoctorLocation mappings are rejected on API writes. Pre-existing duplicate mappings are read together. Existing cross-organization mappings are not rewritten or prohibited by a new schema rule.
- Existing appointments are not moved or erased by working-hour edits or resource deactivation. Cancellation/status-only changes do not rerun new-booking availability checks.

Lifecycle:

```text
booked → arrived → waiting → in_consultation → completed
    Each non-completed state may also transition to cancelled.
    completed and cancelled are terminal.
```

Visits do not silently progress the appointment status. Authorized secretaries/admins use the appointment's explicit actions. Once a visit exists, patient, doctor, location, visit type, and time are locked against reassignment/rescheduling. Parent changes that would contradict clinical descendants are rejected instead of cascading through history.

Cleanup decisions approved on 2026-09-28: an existing Visit's appointment/patient/doctor/location context is immutable. Completing a Visit fills a missing checkout timestamp on the server while preserving supplied/existing checkout times. Completed Visits cannot reopen; authorized users may correct timestamps only. If **any** associated Visit is completed, all further appointment status changes are blocked, including cancellation. Existing appointment statuses are preserved without backfilling or automatic progression. The UI retains history access and authorized booking-note corrections, labels the visit link “View visits,” and hides impossible rescheduling/deletion/lifecycle actions. Multiple Visits per appointment remain supported.

Clinical forms opened in a patient chart preselect a Visit only if there is exactly one scoped candidate, or use an explicitly selected Visit context. A preselected appointment populates new Visit context automatically; derived fields are read-only. Display labels come from scoped response DTO projections, and timestamps use the shared Beirut formatter without changing storage.

All API mutations execute inside a PostgreSQL transaction holding a transaction-scoped advisory lock. Consequently availability validation and booking persistence are serialized across API instances using the same database; two simultaneous overlapping API bookings cannot both succeed. This intentionally coarse lock is a throughput ceiling: replace it with ordered resource/doctor locks if measured write load requires it. Direct SQL writers must follow equivalent locking/integrity rules; no database exclusion constraint was added.

The UI submits the last `updatedAt` on edits. `X-Expected-Updated-At` rejects stale edits to existing timestamped records with 409. Appointment mutations also accept `expectedUpdatedAt` in their DTO. Resources without an existing modification timestamp cannot provide the same stale-edit detection without a separate approved design.

## Date/time API contract

- API appointment and visit instants are UTC ISO 8601 values ending in `Z`.
- Schedules use local `HH:mm:ss`; exceptions use local calendar dates plus wall times.
- Availability requires `doctorId`, `locationId`, `visitTypeId`, and local `date`, and returns UTC start/end pairs only.
- Booking uses the selected returned UTC start; the browser never calculates the appointment end.
- Display and calendar grouping use `Intl.DateTimeFormat` with `Asia/Beirut`, independent of the browser timezone.
- Manual datetime inputs and drag coordinates use explicit Beirut-to-UTC conversion. Invalid or ambiguous daylight-saving times are rejected/omitted rather than choosing an offset silently.
- Reports receive UTC instants representing the selected Beirut date boundaries; the ending instant is exclusive.

## Clinical, payment, file and notification behavior

Appointments, consultation creators, payment collectors, file uploaders and audit actors are server-derived. Updates preserve original attribution. Clinical consistency checks tie visits to appointments, prescriptions to consultations/visits, follow-ups to optional appointments, and next appointments to the consultation patient.

Payments require a positive amount and the existing method/recipient/status values. `Amount` is the amount actually recorded in that record. There are no invoice, balance, receivable, refund, or currency-conversion tables/workflows.

Files support metadata and HTTPS links, **not binary uploads or storage**. Supported parent table names are `patients`, `visits`, `consultations`, `prescriptions`, `follow_ups`, `lab_requests`, `imaging_requests`. Read/write authorization follows the parent operation. No arbitrary table access is constructed from input. External file hosting must enforce its own access controls; possessing an application-authorized link is not a substitute for storage authorization.

Notifications are existing stored records. Users see their own list and can mark their own records read. Admin API operations are supported; no email/SMS delivery infrastructure or automatic reminder sender was invented.

Audit entries are backend-generated with the business transaction and contain actor, table, record ID and action, not passwords, JWTs, diagnoses or request payloads. Ordinary audit create/update/delete routes are removed. Stored role/permission/user-role records remain admin-managed reference data; JWT enforcement continues to use the existing `User.Role` model.

## Reporting definitions

`GET /api/reports/summary` accepts `from`, `to` (UTC, maximum 366-day span), and optional `doctorId`, `locationId`, `status`, `paymentMethod`, `paidTo`.

- Appointment activity: appointment start date, counts grouped by status.
- Visit activity: check-in date, falling back to creation date.
- Completed consultation activity: consultation creation date with the related visit currently completed. Consultation has no independent completion timestamp/status.
- Recorded Collections: sum of `PaymentRecord.Amount` by record creation date, payment method and paid-to value. All recorded amounts count; no invoice totals are inferred.
- Upcoming appointments: non-terminal appointments at/after now within the requested range, ordered by start, capped at 50.

Queries aggregate on the server and inherit authorized doctor scope. Totals do not invent a currency; maintain consistent operational units outside this schema.

## API additions and list contracts

- `GET /api/reports/summary`
- `PATCH /api/appointments/{id}/status`
- `PUT /api/appointments/{id}/reschedule`
- `GET /api/appointments/{id}/availability` for authorized rescheduling, excluding that appointment's existing interval.
- `PATCH /api/notifications/{id}/read`
- `GET /health`

Existing collections retain array responses and accept `page` (default 1), `pageSize` (default 100, maximum 200). Patient search supports names/MRN/phone. Supported relation filters include doctor/patient/location/visit/consultation/prescription/DoctorLocation IDs where relevant. Appointments support `from`, `to`, `status`; clinical resources support patient-context filtering through parent visits/prescriptions; files support `tableName` and `recordId`; notifications support `userId` within the caller's scope.

Display-only name fields and `AppointmentDto.hasVisits` support the UI without downloading unrelated records. Clinical/resource forms use API-derived relationship selectors. Errors use safe ProblemDetails for validation, authentication, authorization, missing resources and conflicts.

## Verification

```powershell
dotnet test backend.Tests/Healthcare.Api.Tests.csproj
cd frontend
npm run typecheck
npm test
npm run build
cd ..
dotnet run --project verification/Healthcare.Verify.csproj
```

Tests use `HEALTHCARE_TEST_CONNECTION` if set, otherwise the ignored local development PostgreSQL connection. They create uniquely named `healthcarev2_test_<random>` databases and drop only those databases after completion. They never migrate, reset or seed the application database. Test accounts/data exist only in that isolated environment.

Full-stack verification starts real Kestrel and Vite processes on unused loopback ports, runs headless Playwright using installed Microsoft Edge, and shuts down the processes afterward. The browser deliberately uses `America/Los_Angeles` while scheduling remains Beirut-local. Install Edge or adjust the local Playwright channel if necessary. No API response mocking is used. Screenshots/traces/reports are written under ignored `test-results/`; retain only appropriate test artifacts.

The verification runner checks `HasPendingModelChanges()` against the unchanged EF snapshot. See the [cleanup report](docs/cleanup-report.md) for current verification results; the Phase 2 report records the earlier baseline.

## Deployment tasks and boundaries

Production build output is `frontend/dist/`. Host it with SPA fallback and route `/api` to ASP.NET Core. Configure actual trusted origins if splitting origins. Supply production secrets outside source control. Configure HTTPS, trusted reverse proxies/forwarded headers, host restrictions, PostgreSQL access, backups/restore procedures, monitoring and retention according to the deployment environment. None of the production domain, TLS certificates, nginx configuration, cloud storage or credentials were invented.

Login has a per-process/per-client-address rate limit. Coordinate client IP forwarding and distributed rate limiting at the actual deployment boundary if using multiple instances. The global mutation lock is database-wide, not per-process.

Redis was not added because no established requirement needs it. Binary file storage, SMS/email delivery, token revocation/refresh, invoices and deployment infrastructure remain explicitly outside the implemented architecture.
