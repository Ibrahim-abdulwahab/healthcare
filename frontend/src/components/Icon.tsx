// Native SVG paths reused from the original OmniSync page; no icon dependency.
const paths: Record<string, string> = {
  dashboard: 'M3 10 12 3 21 10V21H15V13H9V21H3Z',
  calendar: 'M3 5H21V21H3ZM7 3V7M17 3V7M3 10H21M7 14H10M14 14H17M7 18H10',
  appointments: 'M3 5H21V21H3ZM7 3V7M17 3V7M3 10H21M7 16 10 19 17 13',
  patients: 'M6 7A3 3 0 1 0 12 7A3 3 0 1 0 6 7M3 21V18A6 6 0 0 1 15 18V21M17 4A3 3 0 0 1 17 10M18 14A5 5 0 0 1 22 19V21',
  medical: 'M8 21V3H16V21M8 8H3V21H21V8H16M10 7H14M12 5V9M5 12H6M5 16H6M18 12H19M18 16H19M10 21V16H14V21',
  clock: 'M3 12A9 9 0 1 0 21 12A9 9 0 1 0 3 12M12 6V12L17 15',
  reports: 'M4 21V10H8V21M10 21V3H14V21M16 21V7H20V21M3 21H21',
  settings: 'M9 3 8 6 5 7 3 11 5 13V17L9 20 12 19 15 20 19 17V13L21 11 19 7 16 6 15 3ZM9 12A3 3 0 1 0 15 12A3 3 0 1 0 9 12',
  bell: 'M18 8A6 6 0 0 0 6 8C6 15 3 15 3 18H21C21 15 18 15 18 8M10 21H14',
  payments: 'M3 5H21V19H3ZM3 10H21M6 15H10',
  logout: 'M12 2V12M6 5A9 9 0 1 0 18 5',
};
export function Icon({ name }: { name: string }) { return <svg viewBox="0 0 24 24" aria-hidden="true"><path d={paths[name] || paths.calendar} /></svg>; }
