# Phase 2 implementation and verification report

Date: 2026-09-27. Scope: the original Phase 2 request, subsequent continuation requests, and the approved secretary consultation-read clarification.

## 1. Executive summary

Healthcarev2 now has an integrated React/TypeScript application using the existing ASP.NET Core/EF Core/PostgreSQL backend. It supports authenticated role-specific workflows, patient and clinical scopes, booking, availability, day/week calendars, real drag rescheduling, clinical forms, payments, reports, notifications, administrative screens and authorized file metadata.

The backend was extended in place. The original entity models, EF configurations, migration files and snapshot were not changed. No application data was migrated, reset, seeded or deleted. Verification used disposable, randomly named PostgreSQL databases.

The application has application-level hardening and local full-stack verification. This does not certify an unconfigured production deployment or every possible edge case.

## 2. Final repository architecture

```text
React pages/forms/calendar
  → centralized typed bearer-token API client
  → ASP.NET controllers + resource authorization/transaction filter
  → existing services + shared SQL scopes / scheduling / reports
  → HealthcareDbContext + transactional audit entries
  → PostgreSQL, existing schema
```

- Frontend: `frontend/src/App.tsx`, `auth/`, `layout/`, `pages/`, `features/`, `components/`, `api/`, `lib/`.
- Backend: existing controller/service/DTO/EF structure retained, with small `Security/` and `Infrastructure/` additions.
- Verification: `backend.Tests/` and `verification/`, plus `frontend/e2e/`.
- Build: `Healthcarev2.sln`; npm lockfile; existing backend versions pinned to the restored versions used for verification.

## 3–4. Backend and frontend file changes

The exact added/modified/deleted file inventory is appended at the end of this report. Generated `bin`, `obj`, `node_modules`, `dist` and test artifacts are excluded.

The original frontend stylesheet remains the visual foundation. The old in-memory `app.js`, `schedule.js` and demo test are retired; they are not a production application path.

## 5. Schema and migration changes

**IMPLEMENTED + TESTED: preservation of the existing schema.**

- No entity fields, tables, relationships, timestamps, unique constraints or string lengths were added.
- No migration was created, rewritten or applied to the application database.
- `Doctor.Id == User.Id`, staff assignment columns, multiple visits per appointment, polymorphic file references, closed status spellings and restrictive FKs remain.
- `HasPendingModelChanges()` in `verification/Program.cs` returned false.
- Git verification confirmed that `backend/Models/`, `backend/Data/Configurations/` and `backend/Migrations/` have no changes.
- New name/visit-presence fields are response DTO projections only; concurrency expectations are request metadata only.

## 6. Authorization implementation

**IMPLEMENTED + TESTED.**

`Security/RequestUser.cs` reads validated identity and current staff assignments. `ResourceAccess.Query<T>()` translates ownership scopes to SQL. `AccessPolicy.Allows()` establishes operation permissions. `ResourceAccessFilter.OnActionExecutionAsync()` checks the existing resource and requested target parents before the service runs.

Authentication is required by default. Login, time-only public availability, and the minimal health check are explicit exceptions. Administrative resources are admin-only; shared lookup reads are authenticated. Doctors can discover their own DoctorLocations. Staff creation/update targets are checked against their assignments.

The current-source appointment POST discrepancy was resolved with real HTTP tests: the earlier source lacked the assignment check, and the implemented target authorization now rejects both direct and organization secretaries booking outside scope.

## 7. Patient scoping

**IMPLEMENTED + TESTED.**

`ResourceAccess.Query<Patient>()` grants access through non-cancelled appointments or historical visits involving authorized doctors. Cancelled-only relationships do not grant access. Filtering happens in SQL before pagination/projection.

Doctors read but cannot create/update/delete demographics. Secretaries create patients and update connected patients. New registration returns the patient to the booking flow; the appointment establishes ongoing visibility. Admins retain full patient access.

There is no direct Patient doctor/organization FK and no global secretary patient search. Existing MRN collisions outside a secretary's scope require administrative resolution, as documented in the README.

## 8. Clinical scoping

**IMPLEMENTED + TESTED.**

Visits follow their doctor, consultations/lab/imaging/payments follow their visit, prescriptions follow both their doctor and consultation context, prescription items follow their prescription, and follow-ups follow their doctor. Shared patient demographics never unlock another doctor's clinical records.

Secretaries may read scoped consultations and their attachments, following the explicit user clarification. They cannot author consultations, prescriptions/items, follow-ups or lab/imaging requests. Doctors manage their own permitted clinical records; clinical hard deletes remain admin-only.

`VisitService`, `ConsultationService`, `PrescriptionService`, `FollowUpService` and the existing relationship validators reject contradictory patient/doctor/location/parent changes. Parent edits do not silently cascade into clinical history.

## 9. Scheduling and availability

**IMPLEMENTED + TESTED.**

`AvailabilityService.GetAvailableSlotsAsync()` and `SlotCalculator.Calculate()` are authoritative for public availability and appointment creation/rescheduling.

- Visit type determines duration; schedule interval determines start times.
- Active doctor/location/mapping/type/working time is required for new bookings.
- Available exceptions add hours; unavailable exceptions subtract them.
- Occupied appointments block across locations; cancelled appointments do not.
- Overlapping working coverage is merged and duplicate starts removed.
- DateTime arithmetic prevents TimeOnly wraparound/infinite loops.
- Appointment ends are computed by the server.
- Exception interval fallback uses same-weekday mapping interval, then any active mapping interval, then visit duration; no new database field was invented.

The frontend displays and converts dates explicitly in Asia/Beirut. Browser verification ran in America/Los_Angeles and still confirmed 09:00 Beirut persisted as 07:00 UTC in winter, rendered correctly, and rescheduled correctly. Frontend checks also cover summer offsets and calendar boundaries.

Ambiguous/nonexistent daylight-saving wall times are omitted/rejected; full alternative-offset selection is intentionally not implemented.

## 10. Appointment lifecycle

**IMPLEMENTED + TESTED.**

`AppointmentLifecycle.Validate()` allows only the specified forward sequence and cancellation from pre-completion states. Completed/cancelled states are terminal. `AppointmentService.UpdateAsync()` locks demographic/context/time reassignment once visits exist and checks dependent references.

Status-only/cancellation changes bypass new-booking availability checks. Tests cover cancellation after blocking exceptions and deactivation of each relevant resource. Schedule changes preserve existing appointments.

The UI uses action buttons rather than an arbitrary lifecycle dropdown. Visit completion is a separate clinical operation; appointment status is not silently synchronized by a doctor who lacks appointment-write permission.

## 11. Booking concurrency and stale edits

**IMPLEMENTED + TESTED.**

`ResourceAccessFilter` holds `pg_advisory_xact_lock(727410, 1)` inside the mutation transaction. Validation and persistence therefore serialize across API instances using the same PostgreSQL database. The concurrent booking test sends two overlapping requests for one doctor at different locations and verifies exactly one creation and one 409 conflict.

This is a deliberately coarse global mutation lock, with the throughput ceiling documented in code. It is not a process-local mutex. Direct SQL writers outside the API must follow equivalent integrity rules; no exclusion constraint was added.

Timestamped edits support `X-Expected-Updated-At`; appointments additionally accept `expectedUpdatedAt`. The UI supplies the original timestamp and stale writes return 409. Records without existing modification timestamps cannot provide equivalent stale-edit detection without a separate design.

## 12. Actor attribution

**IMPLEMENTED + TESTED.**

Appointment creator, consultation creator, payment collector, file uploader and audit user derive from the authenticated request. Client actor fields were removed from applicable write DTOs; unknown submitted actor properties do not control saved identity. Original attribution is preserved on updates. HTTP tests explicitly attempt spoofed creator/collector/uploader IDs.

## 13. File security

**IMPLEMENTED + TESTED: metadata/link authorization. NOT IMPLEMENTED: binary upload/storage.**

`ResourceAccess.FileParent()` is a fixed whitelist. File queries and target checks inherit parent ownership and operation permissions. Staff consultation reads/attachments are allowed, but clinical attachment writes follow clinical parent-write restrictions. Metadata accepts HTTPS links only; no arbitrary table SQL is constructed.

`FilePanel` supports listing, authorized link creation/editing, and admin deletion of metadata. It explicitly states the storage limitation. Actual hosted-file access must also be protected by the storage system; this project cannot revoke an independently public external URL.

## 14. Administrative/configuration security

**IMPLEMENTED + TESTED at the API permission boundary. IMPLEMENTED + BUILD-VERIFIED for the complete set of admin forms; representative management exercised in-browser.**

Admin screens cover organizations, locations, doctors, users, DoctorLocations, visit types, items/categories, roles, permissions, user roles, settings, code lists and audit history. `features/resources.ts`, `RecordForm`, `ResourcePage` and `Admin` provide forms with real API relationship selectors.

Role/permission tables remain reference records; the established `User.Role` remains the JWT authority. Category ancestor cycles and duplicate active DoctorLocation mappings are rejected. Existing FK restrictions produce safe conflict responses.

Not every CRUD permutation of every administrative resource was individually exercised in the browser. The shared form pipeline and a real visit-type creation were exercised; all screens were type/build-validated.

## 15. Payments

**IMPLEMENTED + TESTED.**

Payment writes follow visit scope, require a positive amount and existing closed values, and preserve the authenticated collector. Doctors have read-only access. `ResourcePage` exposes only real existing payment fields, excluding editable collector identity.

Browser verification recorded 42.50, verified the collector, and observed the resulting 67.50 total including the fixture's existing 25. Reports call these **Recorded Collections**, not revenue, invoices or outstanding balances.

## 16. Audit logging

**IMPLEMENTED + TESTED.**

`HealthcareDbContext.SaveChangesAsync()` writes audit rows in the business transaction using authenticated actor, mapped table, record ID and action. Appointment status/reschedule actions receive meaningful action labels. No sensitive payload, password or token is logged as audit content.

Audit routes are admin read-only. Normal create/update/delete actions were removed. The browser exercised audit inspection and API tests rejected forged audit creation.

## 17. Authentication and application hardening

**IMPLEMENTED + TESTED for authentication behavior; IMPLEMENTED + BUILD/RUNTIME-VERIFIED for local startup/health/error handling.**

`Program.cs` preserves JWT signature/issuer/audience/lifetime validation and zero skew. Its existing user lookup now validates current role and assignment, rejecting stale-role tokens. Temporary admin-test routing is removed. Required configuration is validated; login is rate limited; health checks verify database connectivity; global errors use safe ProblemDetails.

`AuthProvider` centralizes login/restore/logout. Tokens live in tab-scoped sessionStorage and are cleared on 401. No refresh-token/revocation system was added, per the explicit request. Production TLS/proxy/client-IP configuration and distributed abuse controls remain deployment responsibilities.

## 18. Reporting

**IMPLEMENTED + TESTED.**

`ReportService.GetAsync()` aggregates appointment activity, doctor/visit/consultation activity, recorded collections and upcoming appointments over authorized scope. `ReportingTests` verifies scope, counts, payment dates/totals and invalid date ranges. `Reports` renders the real endpoint for dashboards and reports.

The README defines all date bases and the meaning of “completed consultations” (consultation creation date, related visit currently completed). No reporting/billing tables were added.

## 19. API additions and changes

Added:

- `GET /api/reports/summary`
- `PATCH /api/appointments/{id}/status`
- `PUT /api/appointments/{id}/reschedule`
- `GET /api/appointments/{id}/availability`
- `PATCH /api/notifications/{id}/read`
- `GET /health`

Changed:

- Protected former public CRUD and scoped all relevant collections/records.
- Availability requires visitTypeId; booking/rescheduling derives end time.
- Existing collection arrays now support bounded pagination and relevant search/relation/date filters.
- Write DTOs no longer control actor fields; response DTOs include useful display names and appointment visit presence.
- Audit mutation routes and temporary admin-test route removed.
- Consistent safe validation/conflict responses; optional stale-edit preconditions.

## 20. Frontend routes/pages

| Route | Implementation |
|---|---|
| `/login` | `Login`, `AuthProvider` |
| `/` | `Reports` dashboard plus `DashboardContext` |
| `/calendar` | `Calendar` day/week scheduling |
| `/appointments` | `Appointments`, `AppointmentActions` |
| `/availability` | `Booking` in read-only availability mode |
| `/book` | `Booking`, real availability, create/reschedule |
| `/patients` | `ResourcePage` patient demographics |
| `/patients/:id` | `PatientChart`, scoped clinical tabs, attachment links |
| `/records/:resource` | Shared real CRUD forms for allowed resources |
| `/reports` | Server-aggregated reporting |
| `/notifications` | Personal notifications/mark read |
| `/admin` | Admin management navigation |

`App` protects direct navigation, `Shell` makes navigation role-aware, and the API remains authoritative.

## 21. Role-specific frontend behavior

**IMPLEMENTED + TESTED for core workflows/guards; remaining admin form permutations build-verified.**

- Admin: management, patient/appointment/clinical operations, reporting and audit inspection; permanent appointment deletion is explicit and subject to FK constraints.
- Secretary: patient registration/search/edit, booking, calendar movement, lifecycle/check-in, payments and scoped clinical reads.
- Doctor: own appointment reads, authorized charts, clinical authorship, own working hours/exceptions, activity and recorded-collection information.
- Dashboard: upcoming appointments, server metrics, personal notification preview and doctor's schedule summary.
- Direct unauthorized routes show a permission error; expired sessions return to real login.

## 22. Calendar implementation

**IMPLEMENTED + TESTED.**

`Calendar` loads real appointments, doctors, locations, mappings, schedules and exceptions. It uses a day/week grid, location colors, recurring hours, exception overlays and explicit appointment statuses. The visual foundation follows the supplied OmniSync reference; live content, permissions and correct dates replace the reference's sample/inconsistent labels. No pixel-identical screenshot assertion was performed.

Native drag-and-drop calls the real reschedule endpoint. The current event is not moved permanently before acceptance; rejection preserves the accepted position and displays the API reason. “Reschedule” provides a keyboard/form alternative and supports authorized location/doctor choices. Doctors, terminal appointments and appointments with visits cannot be dragged.

## 23. Clinical workflow implementation

**IMPLEMENTED + TESTED for the requested main workflow.**

`PatientChart` opens scoped visit, consultation, prescription, follow-up, lab, imaging and payment tabs. `RecordForm` resolves real relationship choices and derives visit context from selected appointments and prescription context from selected consultations. Browser verification authored all requested clinical resource types, prescription items, and completed the visit. Server fields are not editable.

## 24–26. Final automated verification and builds

| Check | Result |
|---|---|
| `dotnet restore Healthcarev2.sln` | Passed |
| `dotnet build Healthcarev2.sln --no-restore` | Passed; 0 warnings, 0 errors |
| Backend xUnit suite | **64 passed, 0 failed, 0 skipped** |
| `npm run typecheck` | Passed |
| `npm test` | **3 passed, 0 failed** |
| `npm run build` | Passed; production assets generated in `frontend/dist/` |
| Playwright full-stack workflows | **9 passed** |
| EF pending model changes | False |

Backend tests are in `AuthenticationTests`, `AuthorizationTests`, `ClinicalTests`, `SchedulingTests`, `ReportingTests`, `HardeningTests` and `QueryTests`. They combine pure scheduling/lifecycle checks with real JWT/HTTP/PostgreSQL integration cases. Node checks cover timezone/date helpers and role visibility. Playwright uses actual API responses and database writes, not mocks.

Some Windows sandbox attempts blocked compiler/Node child processes or dependency access. Those commands were rerun through the approved execution path; the passing results above are the successful actual runs, not assumptions based on a failed sandbox attempt.

## 27. Actual full-stack workflows exercised

`frontend/e2e/workflows.spec.ts`, run by `verification/Program.cs`:

1. Secretary login → new patient → authorized doctor/location/type → real availability → booking → calendar, with persisted actor and UTC end checks.
2. Native drag accepted and persisted; a cross-location collision returned 409 and retained the previous position.
3. Secretary visit creation → payment recording → recorded-collection report; visit locked rescheduling; UI cancellation freed the time.
4. Doctor chart → consultation → authorized attachment link → prescription/items → follow-up → lab/imaging → visit completion; uploader attribution verified; appointment drag and booking route denied.
5. Own schedule creation and unavailable/available exception creation; availability subtraction/addition checked via the real API.
6. Admin management creation, reports and audit views; desktop calendar and mobile patient screenshots.
7. Personal notification read/mark-read/logout; organization secretary consultation reads with no clinical write controls.
8. Unrelated staff, another doctor, anonymous access, attachment isolation, schedule ownership, patient history/cancellation scope, report scope and notification isolation checked through actual server requests from the browser test environment.
9. Real duplicate-MRN 400 UI feedback, scoped-record 404, forbidden-route feedback and invalid-token 401 session clearing. The earlier drag workflow covers real 409 feedback; API checks cover 403.

The browser timezone is deliberately Los Angeles while all scheduling remains Beirut-local. Screenshots and failed-run debugging artifacts are ignored under `test-results/`. The test runner stops child servers and drops only its randomly named test database after running.

## 28. Documentation

`README.md` documents architecture, local setup, PostgreSQL expectations, development settings, auth/scopes, schedules/exceptions/lifecycle, timezone contracts, reports, file limits, API changes, tests and production considerations. `frontend/README.md` replaces the stale demo instructions. `backend/Healthcare.Api.http` no longer points to the removed weather endpoint. `frontend/.env.example` contains only non-secret configuration guidance.

## 29–31. Limitations, deployment tasks, and status distinctions

**IMPLEMENTED + TESTED:** core authorization, patient/clinical scopes, scheduling/lifecycle/concurrency, actor attribution, file metadata permissions, payments/audit, reporting, JWT role invalidation, main frontend workflows, real backend errors and cross-timezone scheduling.

**IMPLEMENTED + BUILD-VERIFIED:** the full set of administrative and clinical form definitions/routes; shared behavior and representative resources were runtime-tested, but every field/CRUD combination was not exhaustively exercised.

**IMPLEMENTED BUT NOT RUNTIME-VERIFIED:** behavior of the implemented application hardening and concurrency code under production multi-instance load and an actual TLS/proxy/CORS topology. Local multiple concurrent requests were tested; a production cluster/load test was not performed. Production hosting infrastructure itself was not configured.

**NOT IMPLEMENTED, intentionally outside the established scope:** binary file storage/upload, external email/SMS delivery, refresh tokens/revocation, invoices/outstanding balances, Redis architecture, nginx/domain/TLS/cloud-storage configuration. The prompt explicitly prohibited inventing these or required honest limitation reporting.

**BLOCKED:** no remaining local build/test blocker. Production deployment and external file/delivery infrastructure require actual environment choices and credentials; none were guessed.

Practical limits: the advisory lock serializes all API mutations; direct SQL writers are outside that lock contract; sessionStorage bearer tokens still require XSS protection; stateless logout does not invalidate copied tokens; patient registration collisions outside scope require admin resolution; ambiguous DST times are not selectable; frontend tables scroll horizontally on narrow screens; existing historical inconsistent rows are not automatically rewritten; records lacking modification timestamps cannot have equivalent stale-edit checks without an approved extension.

The implemented application fulfills the defined local full-stack workflows. It is not represented as a fully deployed or independently security-certified production healthcare system.

## Exact source file inventory

Generated below from the final working tree. Paths are relative to the repository root.

### Backend source

- **Modified** `backend/Controllers/AppointmentsController.cs`
- **Modified** `backend/Controllers/AuditLogsController.cs`
- **Modified** `backend/Controllers/AuthController.cs`
- **Modified** `backend/Controllers/AvailabilityController.cs`
- **Modified** `backend/Controllers/DoctorLocationsController.cs`
- **Modified** `backend/Controllers/DoctorsController.cs`
- **Modified** `backend/Controllers/LocationsController.cs`
- **Modified** `backend/Controllers/NotificationsController.cs`
- **Added** `backend/Controllers/ReportsController.cs`
- **Modified** `backend/DTOs/Appointments/AppointmentDto.cs`
- **Added** `backend/DTOs/Appointments/ChangeAppointmentStatusDto.cs`
- **Modified** `backend/DTOs/Appointments/CreateAppointmentDto.cs`
- **Added** `backend/DTOs/Appointments/RescheduleAppointmentDto.cs`
- **Modified** `backend/DTOs/Appointments/UpdateAppointmentDto.cs`
- **Modified** `backend/DTOs/Consultations/ConsultationDto.cs`
- **Modified** `backend/DTOs/Consultations/CreateConsultationDto.cs`
- **Modified** `backend/DTOs/Consultations/UpdateConsultationDto.cs`
- **Modified** `backend/DTOs/DoctorLocations/DoctorLocationDto.cs`
- **Modified** `backend/DTOs/HealthcareFiles/CreateHealthcareFileDto.cs`
- **Modified** `backend/DTOs/HealthcareFiles/UpdateHealthcareFileDto.cs`
- **Modified** `backend/DTOs/ImagingRequests/CreateImagingRequestDto.cs`
- **Modified** `backend/DTOs/ImagingRequests/UpdateImagingRequestDto.cs`
- **Modified** `backend/DTOs/ItemCategories/CreateItemCategoryDto.cs`
- **Modified** `backend/DTOs/ItemCategories/UpdateItemCategoryDto.cs`
- **Modified** `backend/DTOs/LabRequests/CreateLabRequestDto.cs`
- **Modified** `backend/DTOs/LabRequests/UpdateLabRequestDto.cs`
- **Modified** `backend/DTOs/PaymentRecords/CreatePaymentRecordDto.cs`
- **Modified** `backend/DTOs/PaymentRecords/UpdatePaymentRecordDto.cs`
- **Modified** `backend/DTOs/PrescriptionItems/CreatePrescriptionItemDto.cs`
- **Modified** `backend/DTOs/PrescriptionItems/UpdatePrescriptionItemDto.cs`
- **Modified** `backend/DTOs/Prescriptions/PrescriptionDto.cs`
- **Added** `backend/DTOs/Reports/ReportDto.cs`
- **Modified** `backend/DTOs/Visits/VisitDto.cs`
- **Modified** `backend/Data/HealthcareDbContext.cs`
- **Modified** `backend/Healthcare.Api.csproj`
- **Modified** `backend/Healthcare.Api.http`
- **Added** `backend/Infrastructure/ApiException.cs`
- **Added** `backend/Infrastructure/ApiExceptionHandler.cs`
- **Added** `backend/Infrastructure/DatabaseHealthCheck.cs`
- **Modified** `backend/Program.cs`
- **Added** `backend/Security/AccessPolicy.cs`
- **Added** `backend/Security/RequestUser.cs`
- **Added** `backend/Security/ResourceAccess.cs`
- **Added** `backend/Security/ResourceAccessFilter.cs`
- **Added** `backend/Services/Appointments/AppointmentLifecycle.cs`
- **Modified** `backend/Services/Appointments/AppointmentService.cs`
- **Modified** `backend/Services/Appointments/IAppointmentService.cs`
- **Modified** `backend/Services/AuditLogs/AuditLogService.cs`
- **Modified** `backend/Services/AuditLogs/IAuditLogService.cs`
- **Modified** `backend/Services/Availability/AvailabilityService.cs`
- **Modified** `backend/Services/Availability/IAvailabilityService.cs`
- **Added** `backend/Services/Availability/SlotCalculator.cs`
- **Modified** `backend/Services/CodeLists/CodeListService.cs`
- **Modified** `backend/Services/Consultations/ConsultationService.cs`
- **Modified** `backend/Services/DoctorLocations/DoctorLocationService.cs`
- **Modified** `backend/Services/Doctors/DoctorService.cs`
- **Modified** `backend/Services/FollowUps/FollowUpService.cs`
- **Modified** `backend/Services/HealthcareFiles/HealthcareFileService.cs`
- **Modified** `backend/Services/ImagingRequests/ImagingRequestService.cs`
- **Modified** `backend/Services/ItemCategories/ItemCategoryService.cs`
- **Modified** `backend/Services/Items/ItemService.cs`
- **Modified** `backend/Services/LabRequests/LabRequestService.cs`
- **Modified** `backend/Services/Locations/LocationService.cs`
- **Modified** `backend/Services/Notifications/INotificationService.cs`
- **Modified** `backend/Services/Notifications/NotificationService.cs`
- **Modified** `backend/Services/Organizations/OrganizationService.cs`
- **Modified** `backend/Services/Patients/PatientService.cs`
- **Modified** `backend/Services/PaymentRecords/PaymentRecordService.cs`
- **Modified** `backend/Services/Permissions/PermissionService.cs`
- **Modified** `backend/Services/PrescriptionItems/PrescriptionItemService.cs`
- **Modified** `backend/Services/Prescriptions/PrescriptionService.cs`
- **Added** `backend/Services/Reports/ReportService.cs`
- **Modified** `backend/Services/Roles/RoleService.cs`
- **Modified** `backend/Services/ScheduleExceptions/ScheduleExceptionService.cs`
- **Modified** `backend/Services/Schedules/ScheduleService.cs`
- **Modified** `backend/Services/SystemSettings/SystemSettingService.cs`
- **Modified** `backend/Services/UserRoles/UserRoleService.cs`
- **Modified** `backend/Services/Users/UserService.cs`
- **Modified** `backend/Services/VisitTypes/VisitTypeService.cs`
- **Modified** `backend/Services/Visits/VisitService.cs`

### Backend tests

- **Added** `backend.Tests/ApiFixture.cs`
- **Added** `backend.Tests/AuthenticationTests.cs`
- **Added** `backend.Tests/AuthorizationTests.cs`
- **Added** `backend.Tests/ClinicalTests.cs`
- **Added** `backend.Tests/HardeningTests.cs`
- **Added** `backend.Tests/Healthcare.Api.Tests.csproj`
- **Added** `backend.Tests/QueryTests.cs`
- **Added** `backend.Tests/ReportingTests.cs`
- **Added** `backend.Tests/SchedulingTests.cs`

### Frontend

- **Added** `frontend/.env.example`
- **Modified** `frontend/README.md`
- **Retired** `frontend/app.js`
- **Added** `frontend/e2e/workflows.spec.ts`
- **Modified** `frontend/index.html`
- **Added** `frontend/package-lock.json`
- **Added** `frontend/package.json`
- **Added** `frontend/playwright.config.ts`
- **Retired** `frontend/schedule.js`
- **Retired** `frontend/schedule.test.mjs`
- **Added** `frontend/src/App.tsx`
- **Added** `frontend/src/api/client.ts`
- **Added** `frontend/src/api/types.ts`
- **Added** `frontend/src/auth/Auth.tsx`
- **Added** `frontend/src/components/Common.tsx`
- **Added** `frontend/src/components/Icon.tsx`
- **Added** `frontend/src/components/RecordForm.tsx`
- **Added** `frontend/src/features/AppointmentActions.tsx`
- **Added** `frontend/src/features/DashboardContext.tsx`
- **Added** `frontend/src/features/FilePanel.tsx`
- **Added** `frontend/src/features/ResourcePage.tsx`
- **Added** `frontend/src/features/resources.ts`
- **Added** `frontend/src/layout/Shell.tsx`
- **Added** `frontend/src/lib/permissions.ts`
- **Added** `frontend/src/lib/time.ts`
- **Added** `frontend/src/main.tsx`
- **Added** `frontend/src/pages/Admin.tsx`
- **Added** `frontend/src/pages/Appointments.tsx`
- **Added** `frontend/src/pages/Booking.tsx`
- **Added** `frontend/src/pages/Calendar.tsx`
- **Added** `frontend/src/pages/Login.tsx`
- **Added** `frontend/src/pages/Notifications.tsx`
- **Added** `frontend/src/pages/PatientChart.tsx`
- **Added** `frontend/src/pages/Reports.tsx`
- **Added** `frontend/src/styles.css`
- **Added** `frontend/tests/time.test.ts`
- **Added** `frontend/tsconfig.json`
- **Added** `frontend/vite.config.ts`

### Verification host

- **Added** `verification/Healthcare.Verify.csproj`
- **Added** `verification/Program.cs`

### Root/support files

- **Added** `.gitignore`
- **Added** `Healthcarev2.sln`
- **Added** `README.md`
- **Added** `docs/phase-2-report.md`
