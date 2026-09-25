# OmniSync frontend

Run from the project root:

```sh
python -m http.server 5173 --bind 127.0.0.1 --directory frontend
```

Open http://localhost:5173. No install or build step. Serve the files over HTTP; browser ES modules do not support opening `index.html` directly from disk.

- `index.html`: page shell and accessible native dialog.
- `styles.css`: layout, colors, calendar, and responsive rules.
- `schedule.js`: sample records and date/scheduling helpers.
- `app.js`: rendering and interactions, grouped by feature.

The initial week is 14–20 September 2026 (the reference image's weekday/date combination is inconsistent). Navigate weeks, switch to a day view, choose a date, inspect/cancel appointments, edit recurring hours, and add or remove date exceptions. Other sidebar sections show simple views of the same demo records. Short screens scroll; narrow calendars scroll horizontally.

Changes are in memory and reset on refresh. All patients are sample data. There is no authentication, database connection, or implied save to the backend. The current backend contains models but no API routes. Add authenticated routes before connecting real records; replace sample data imports with API calls when those routes exist. No patient information is written to browser storage.

Working hours use Monday=0 through Sunday=6. The visible day is 07:00–19:00; sessions must be at least one hour. Appointments are listed compactly inside their location block with explicit start times. Exceptions replace the entire date's recurring hours, and edits that orphan appointments are rejected. Calendar times are displayed as Beirut wall time.

Run the dependency-free scheduling check:

```sh
node frontend/schedule.test.mjs
```
