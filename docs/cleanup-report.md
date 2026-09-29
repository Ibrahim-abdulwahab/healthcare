# Healthcarev2 cleanup and polish report

Date: 2026-09-28

This pass addresses the ten manual findings on top of the committed Phase 2 application. It preserves the controller/service/EF architecture, resource scoping, scheduling calculation and existing schema. No application database reset, data repair, backfill or migration was performed. Appointment 9 was not edited or deleted.

Two lifecycle decisions were explicitly confirmed by the user during this pass:

1. Completed Visits permit timestamp corrections only; they cannot reopen.
2. An appointment's status cannot change while any associated Visit is completed.

## 1. Vite development proxy

**Cause:** `frontend/vite.config.ts` defaulted to the HTTPS profile, while normal development launches the HTTP profile.

**Changes:** `frontend/vite.config.ts`, `frontend/.env.example`, `README.md`.

**Behavior:** plain `npm run dev` now proxies `/api` to `http://localhost:5252`. `API_PROXY_TARGET` still overrides this for HTTPS or other local ports. The browser's production API base remains the environment-configurable `/api`; the development target is not embedded in the production client.

**Verification:** frontend type/build checks; the real E2E runner continues to override the proxy to its isolated API port successfully.

## 2. Raw reference IDs

**Cause:** the shared resource renderer can use `<reference>Name`, but several DTOs contained only foreign-key IDs. Visit option labels also fell back to record numbers instead of clinical context.

**Changes:**

- `backend/DTOs/Schedules/ScheduleDto.cs` and `Services/Schedules/ScheduleService.cs`: `DoctorLocationName`.
- `backend/DTOs/ScheduleExceptions/ScheduleExceptionDto.cs` and `Services/ScheduleExceptions/ScheduleExceptionService.cs`: `DoctorLocationName`.
- `backend/DTOs/AuditLogs/AuditLogDto.cs` and `Services/AuditLogs/AuditLogService.cs`: actor name/email in `UserName`.
- `backend/DTOs/Users/UserDto.cs` and `Services/Users/UserService.cs`: assigned doctor/organization names.
- `backend/DTOs/PaymentRecords/PaymentRecordDto.cs` and `Services/PaymentRecords/PaymentRecordService.cs`: visit patient/doctor label, visit check-in/creation timestamp, collector name.
- `frontend/src/api/types.ts`, `lib/presentation.ts`, `components/RecordForm.tsx`, `features/ResourcePage.tsx`.

**Behavior:** the reported references display doctor/location names, audit actors, staff assignments and patient/doctor/date visit context. Numeric IDs remain in the contracts for relationships. Labels are projected through the existing scoped SQL queries; no broader user/data access was introduced. Mutations return the same stored, enriched projection as reads where applicable. Supplementary name fields are not duplicated in the details display.

**Tests:** `CleanupTests.Display_projections_resolve_labels_without_changing_relationships`; frontend presentation tests; browser checks of schedules, audit, staff details and payments.

Technical record IDs such as the audit target's `RecordId` remain where they are administrative references. This pass does not replace FK architecture or invent unavailable record descriptions.

## 3. New Visit context from Appointment

**Cause:** `RecordForm.change()` fetched appointment context only after a manual dropdown change. A prefilled `appointmentId` from the Check-in link never invoked it. The relation selector also did not reload when an externally populated value changed.

**Changes:** `frontend/src/components/RecordForm.tsx`; E2E appointment-to-visit workflow.

**Behavior:** the shared form resolves appointment context both on initial selection and subsequent selection changes. Patient, doctor and location populate automatically and are read-only for a new Visit. Save waits for context resolution and is blocked if resolution fails. Stale asynchronous responses are ignored. Backend appointment/visit consistency validation remains authoritative and unchanged for creation.

**Tests:** the existing secretary E2E flow now opens Visits with the appointment query parameter and saves without manually selecting patient/doctor/location. Backend inconsistent-creation rejection is explicitly checked in `CleanupTests`.

## 4. Relevant Visit preselection

**Cause:** patient-chart actions supplied a patient ID but did not supply or resolve a Visit ID.

**Changes:** `frontend/src/components/RecordForm.tsx`, `features/ResourcePage.tsx`, `pages/PatientChart.tsx`.

**Behavior:** Consultation, Lab Request and Imaging Request creation preselect a Visit only when the scoped patient query returns exactly one candidate. The query fetches at most two rows to detect ambiguity. Zero/multiple candidates do not produce an arbitrary selection. An explicitly selected chart Visit context or `visitId` route context takes precedence. The chart explains that its optional selection prefills creation forms; it does not imply that all displayed history is filtered to that Visit.

**Tests:** E2E checks for automatic consultation/lab/imaging selection, no guessing with two Visits, and explicit context overriding ambiguity.

## 5. Consistent timestamp presentation

**Cause:** `ResourcePage.display()` formatted only a short list of timestamp fields. `updatedAt` and other timestamp details fell through to raw string rendering.

**Changes:** `frontend/src/lib/presentation.ts`, `features/ResourcePage.tsx`, `components/RecordForm.tsx`.

**Behavior:** resource timestamps, including Updated At, use the existing Asia/Beirut formatter. Date-only fields retain their calendar date and display a friendly date without UTC date shifts. Visit relationship labels include formatted context. If an existing datetime form field is unchanged, its original UTC value is submitted unchanged, preserving seconds and fractional precision instead of truncating it through a minute-only input.

**Tests:** winter/summer presentation checks, unchanged existing timezone tests, browser friendly Updated At assertion, and exact checkout preservation after saving an unchanged form. UTC persistence was not changed.

## 6. Automatic checkout on completion

**Cause:** `VisitService` copied `dto.CheckOutTime` directly and had no completion-time behavior.

**Changes:** `backend/Services/Visits/VisitService.cs`.

**Behavior:** transitioning an in-progress Visit to completed fills checkout with `DateTime.UtcNow` if neither the request nor stored Visit supplies one. Creating an already-completed Visit likewise fills a missing checkout. Explicit supplied times take precedence, and null/omitted input does not erase an existing checkout. Normal ordering and UTC validation still apply. Existing completed rows are not backfilled by a cleanup script.

The service now returns the stored projection after writes. A regression test exposed the difference between .NET 100ns precision and PostgreSQL microseconds in the immediate response; reading the saved projection makes the response consistent with subsequent reads and preserves the enriched labels.

**Tests:** completion time range, one-time population, retained stored time, explicit checkout preservation, and exact timestamp correction behavior in `CleanupTests`; browser completion and unchanged-save checks.

## 7. Authentication expiry UX

**Cause:** the 401 handler correctly cleared credentials, but redirected without preserving an explanatory notice. There was no advance warning or preflight check for already-known token expiry.

**Changes:** `frontend/src/lib/session.ts`, `api/client.ts`, `auth/Auth.tsx`, `pages/Login.tsx`, `layout/Shell.tsx`, `components/Common.tsx`, `README.md`.

**Behavior:**

- The local development lifetime remains **60 minutes** (`Jwt:ExpirationMinutes`), with existing zero server clock skew.
- The UI warns during the last two minutes, including inside modal forms.
- Known-expired tokens are rejected before another API submission; actual 401 responses remain authoritative and clear the session.
- Login explains that the session expired/is invalid and unsaved form entries are not retained. If a save was already in progress, it asks the user to check the record before retrying rather than claiming that no request reached the server.
- Late responses from an older token cannot clear or restore a newly signed-in session.
- No refresh tokens, lifetime extension, cookie redesign or persistent clinical drafts were added.

Production lifetime remains deployment configuration, not a new hard-coded value. Client expiry parsing is only a display/preflight convenience; it does not authenticate a token or replace server validation.

**Tests:** defensive expiry-parser checks; the original real 401 E2E case; browser clock advancement verifying a visible warning inside a form followed by expiry/login notice.

## 8. Completed Visit edit actions

**Cause:** general Visit update permission intentionally included completed Visits, and the UI used a generic Edit action without expressing the distinction between correction and reopening.

**Decision:** preserve authorized timestamp corrections, but prohibit reopening, as explicitly chosen by the user.

**Changes:** `backend/Services/Visits/VisitService.cs`, `frontend/src/components/RecordForm.tsx`, `features/ResourcePage.tsx`.

**Behavior:** a completed Visit shows **Correct timestamps**. Its status and identity/context fields are disabled; check-in/checkout corrections remain available to already-authorized users. The server rejects a completed-to-in_progress change with 409. Doctor/staff/admin resource scope and hard-delete permissions were not broadened.

**Tests:** backend reopening rejection and successful corrections; browser action label, disabled fields and preserved timestamp checks.

## 9. Immutable existing Visit context

**Cause:** the previous backend blocked reassignment only after clinical/payment children existed. The UI exposed all context fields on every edit.

**Changes:** `backend/Services/Visits/VisitService.cs`, `frontend/src/components/RecordForm.tsx`.

**Behavior:** existing Visits cannot change appointment, patient, doctor or location, even without descendants. The UI disables these selectors and explains why. The server rejects direct inconsistent/reassignment requests. New Visit creation continues to accept the existing DTO contract and validates its context against the appointment.

**Tests:** attempted reassignment to another otherwise-valid appointment within the same scope, contradictory creation, and read-only edit controls in the browser.

## 10. Appointment actions with a completed Visit

**Cause:** the appointment UI considered only appointment status and `hasVisits`. The server's appointment state machine was deliberately independent of Visit completion. Thus a booked appointment with completed clinical work could still offer its ordinary next lifecycle action.

**Decision:** any currently associated completed Visit locks further appointment status changes, as explicitly chosen by the user. No automatic status jump or data reconciliation was authorized.

**Changes:** `backend/DTOs/Appointments/AppointmentDto.cs`, `Services/Appointments/AppointmentLifecycle.cs`, `Services/Appointments/AppointmentService.cs`, `Services/Reports/ReportService.cs`, `frontend/src/api/types.ts`, `lib/permissions.ts`, `features/AppointmentActions.tsx`.

**Behavior:**

| Action | Appointment with a completed Visit |
|---|---|
| Mark arrived / further progression | Hidden; both status and full-update API routes reject changes with 409 |
| Cancel | Hidden; server rejects with 409 |
| Reschedule / change clinical context | Still forbidden |
| Delete appointment | Hidden when any Visit exists; explicit server conflict preserves existing FK protection |
| Booking-note correction | Retained for authorized admin/staff; it does not alter appointment status |
| Open patient chart | Retained under existing patient scope |
| Visit navigation | Retained as **View visits**, rather than suggesting a new check-in |

`hasCompletedVisits` is a response-only projection from existing Visits. Multiple Visits remain supported; one completed Visit suffices even if another remains in progress. Existing appointment status is retained, including legacy booked/completed-Visit combinations. Appointment 9 was not changed or used as a test fixture.

**Tests:** backend tests for admin/direct secretary/organization secretary, both write routes, unchanged status and allowed note corrections; browser action visibility and real 409 response on an isolated appointment fixture. Existing ordinary lifecycle, scheduling and cancellation-after-schedule-change tests continue to run.

## Intentionally unchanged

- JWT lifetime, signature validation, active-user/current-role validation, secretary assignment architecture and authorization scopes.
- Appointment duration, start alignment, exceptions, timezone conversion, cross-location conflicts and booking locking.
- Persisted appointment statuses and all existing application data.
- Multiple Visits per appointment and existing restrictive FKs.
- Doctor access to authorized completed Visits for the approved timestamp corrections.
- No refresh-token infrastructure or persisted unsaved clinical drafts.

## API/UI compatibility

No request DTO fields or endpoints were removed. Response DTO additions provide display labels and `hasCompletedVisits`. Visit completion now supplies a missing checkout, existing Visit context is immutable, completed Visits cannot reopen, and appointment status changes are blocked while an associated completed Visit exists. These are the requested and explicitly approved behavioral changes. Unchanged appointment status permits authorized booking-note corrections.

## Data and EF verification

No models, EF configurations or migrations changed. No migration was created or applied. The existing verification runner checks `HasPendingModelChanges()` against the original snapshot. Automated tests create/drop only their own randomly named test databases; they do not reset or seed the application database.

## Verification results

- Backend restore: passed.
- Solution build: passed, zero warnings/errors.
- Full backend suite: **75 passed, 0 failed, 0 skipped** (64 original plus 11 cleanup cases).
- Frontend typecheck: passed.
- Frontend tests: **6 passed** (three existing checks plus three cleanup checks).
- Production frontend build: passed.
- Real Playwright/API/PostgreSQL E2E: **13 passed** (nine existing workflows plus four cleanup workflows).
- EF pending model changes: **false**.

The existing E2E booking/clinical workflows were strengthened to exercise preselected appointment/Visit context rather than bypassing the findings with manual selections. Existing authorization/scheduling assertions were retained.

## Remaining limitations

- A legacy appointment may remain booked while its Visit is completed. Its status is now locked, as requested. Reconciling such data would need a separately approved business/data-repair decision; this cleanup does not silently rewrite it.
- Session expiry still requires signing in again. Unsaved forms are not persisted; the warning/notice prevents silent loss being mistaken for a successful save.
- Display labels reflect current referenced names, not a historical name snapshot. Adding historical snapshots would be a persistence change and was not introduced.
- The existing Phase 2 deployment/file-storage/delivery limitations remain; see `README.md`. This pass did not redesign or deploy infrastructure.
