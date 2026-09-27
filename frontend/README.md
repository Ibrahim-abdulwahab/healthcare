# OmniSync frontend

React + TypeScript with Vite. All production records come from the ASP.NET Core API.

```powershell
npm ci
npm run dev
npm run typecheck
npm test
npm run build
```

See [the root README](../README.md) for backend configuration, authorization rules, timezone contracts, API changes, deployment considerations, and full-stack verification.

- `src/api/`: centralized authenticated requests and DTO types.
- `src/auth/`: per-tab JWT session and identity restoration.
- `src/pages/`: dashboards/reports, calendar, booking, patients, notifications and administration.
- `src/features/`: resource forms, patient-context clinical workflows, attachments and appointment actions.
- `src/lib/`: permission visibility and explicit Beirut timezone conversion.
- `e2e/`: real browser/API/PostgreSQL workflows; run through `verification/Healthcare.Verify.csproj` from the repository root.

The previous in-memory demo JavaScript and demo tests were retired. The original stylesheet is retained as the visual foundation, supplemented by `src/styles.css`.
