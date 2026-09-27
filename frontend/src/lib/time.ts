export const zone = 'Asia/Beirut';
function parts(date: Date) {
  return Object.fromEntries(new Intl.DateTimeFormat('en-GB', { timeZone: zone, year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23' }).formatToParts(date).map(p => [p.type, p.value]));
}
export function dateKey(date = new Date()): string { const p = parts(date); return `${p.year}-${p.month}-${p.day}`; }
export function clockTime(iso: string): string { const p = parts(new Date(iso)); return `${p.hour}:${p.minute}`; }
export function displayTime(iso?: string | null): string { return iso ? new Intl.DateTimeFormat('en-GB', { timeZone: zone, day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(iso)) : '—'; }
export function addDays(date: string, count: number): string { const d = new Date(`${date}T12:00:00Z`); d.setUTCDate(d.getUTCDate() + count); return d.toISOString().slice(0, 10); }
export function monday(date: string): string { return addDays(date, -((new Date(`${date}T12:00:00Z`).getUTCDay() + 6) % 7)); }
export function weekday(date: string): number { return new Date(`${date}T12:00:00Z`).getUTCDay(); }
export function shortDate(date: string): string { return new Intl.DateTimeFormat('en-GB', { timeZone: 'UTC', day: 'numeric', month: 'short' }).format(new Date(`${date}T12:00:00Z`)); }
export function minutes(time: string): number { const [h, m] = time.split(':').map(Number); return h * 60 + m; }
export function localInput(iso: string): string { return `${dateKey(new Date(iso))}T${clockTime(iso)}`; }
export function toUtc(local: string): string {
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(local)) throw new Error('Enter a valid Beirut date and time.');
  const nominal = Date.parse(`${local}:00Z`);
  const offsets = new Set<number>();
  for (const hours of [-36, 0, 36]) {
    const instant = new Date(nominal + hours * 3600000); const p = parts(instant);
    offsets.add(Date.parse(`${p.year}-${p.month}-${p.day}T${p.hour}:${p.minute}:${p.second}Z`) - instant.getTime());
  }
  const matches = [...offsets].map(offset => new Date(nominal - offset)).filter(d => localInput(d.toISOString()) === local);
  if (matches.length !== 1) throw new Error('This Beirut time is missing or ambiguous because of daylight saving. Choose another time.');
  return matches[0].toISOString();
}
export function dayStart(date: string): string {
  for (let minute = 0; minute < 180; minute++) {
    try { return toUtc(`${date}T${String(Math.floor(minute / 60)).padStart(2, '0')}:${String(minute % 60).padStart(2, '0')}`); } catch { /* Midnight can be skipped at DST start. */ }
  }
  throw new Error('Cannot determine the start of this scheduling date.');
}
