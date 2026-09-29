import type { DataRow } from '../api/types.ts';
import { displayTime } from './time.ts';

const timestampFields = new Set(['createdAt', 'updatedAt', 'startDatetime', 'endDatetime', 'checkInTime', 'checkOutTime', 'visitDatetime', 'expiresAt']);
export function displayValue(row: DataRow, key: string): string {
  const value = row[key];
  if (value === null || value === undefined || value === '') return '—';
  if (key === 'visitId' && row.visitName) return `${row.visitName}${row.visitDatetime ? ` · ${displayTime(String(row.visitDatetime))}` : ''}`;
  const name = row[`${key}Name`] || (key.endsWith('Id') ? row[`${key.slice(0, -2)}Name`] : undefined);
  if (name) return String(name);
  if (timestampFields.has(key)) return displayTime(String(value));
  if ((key === 'date' || key === 'dateOfBirth' || key.endsWith('Date')) && /^\d{4}-\d{2}-\d{2}$/.test(String(value)))
    return new Intl.DateTimeFormat('en-GB', { timeZone: 'UTC', day: 'numeric', month: 'short', year: 'numeric' }).format(new Date(`${value}T12:00:00Z`));
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  return String(value);
}

export function rowLabel(row: DataRow): string {
  if (row.doctorName && row.locationName && !row.patientName) return `${row.doctorName} — ${row.locationName}`;
  if (row.patientName) {
    const time = row.startDatetime || row.checkInTime || row.createdAt;
    return [row.patientName, row.doctorName, time ? displayTime(String(time)) : null, row.locationName].filter(Boolean).join(' · ');
  }
  if (row.name) return `${row.name}${row.email ? ` (${row.email})` : ''}`;
  if (row.firstName) return `${row.firstName} ${row.lastName || ''}${row.mrn ? ` · ${row.mrn}` : ''}`;
  return String(row.testName || row.serviceName || row.reasonForVisit || row.key || row.code || row.action || `Record #${row.id}`);
}
