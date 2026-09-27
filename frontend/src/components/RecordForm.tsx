import { useState, type FormEvent } from 'react';
import { api, query } from '../api/client';
import type { DataRow } from '../api/types';
import type { Field } from '../features/resources';
import { ErrorMessage, useLoad } from './Common';
import { dateKey, displayTime, localInput, toUtc } from '../lib/time';

export function rowLabel(row: DataRow): string {
  if (row.doctorName && row.locationName && !row.patientName) return `${row.doctorName} · ${row.locationName}`;
  if (row.patientName) return `${row.patientName}${row.startDatetime ? ` · ${displayTime(String(row.startDatetime))}` : ` · #${row.id}`}${row.locationName ? ` · ${row.locationName}` : ''}`;
  if (row.name) return String(row.name);
  if (row.firstName) return `${row.firstName} ${row.lastName || ''}${row.mrn ? ` · ${row.mrn}` : ''}`;
  if (row.doctorId && row.locationId) return `${row.startDatetime ? displayTime(String(row.startDatetime)) : 'Doctor / location'} · Doctor ${row.doctorId} · Location ${row.locationId}`;
  if (row.prescriptionDate) return `${row.prescriptionDate} · Patient ${row.patientId}`;
  return `${row.testName || row.serviceName || row.reasonForVisit || row.key || row.code || row.action || 'Record'} #${row.id}`;
}
export function RelationSelect({ field, value, change, patientId, disabled }: { field: Field; value: unknown; change: (v: string) => void; patientId?: number; disabled?: boolean }) {
  const [search, setSearch] = useState(''); const [page, setPage] = useState(1);
  const { data, loading, error } = useLoad(async () => {
    const rows = await api<DataRow[]>(`/${field.relation}?${query({ search, page, pageSize: 100, patientId: ['appointments', 'visits', 'consultations', 'prescriptions'].includes(field.relation || '') ? patientId : undefined })}`);
    if (value && !rows.some(r => r.id === Number(value))) { const selected = await api<DataRow>(`/${field.relation}/${value}`); rows.unshift(selected); }
    if (field.relation === 'users' && field.key === 'userId' && field.label === 'Doctor account') return rows.filter(r => r.role === 'doctor');
    return rows;
  }, [field.relation, search, page, patientId]);
  return <div className="relation-input"><input aria-label={`Search ${field.label.toLowerCase()} options`} placeholder={`Search ${field.label.toLowerCase()}…`} value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} disabled={disabled} /><select aria-label={field.label} required={field.required} value={value == null ? '' : String(value)} onChange={e => change(e.target.value)} disabled={disabled || loading}><option value="">{loading ? 'Loading…' : `Select ${field.label.toLowerCase()}`}</option>{data?.map(row => <option key={row.id} value={row.id}>{rowLabel(row)}</option>)}</select><div className="option-pages"><button type="button" disabled={page === 1 || loading} onClick={() => setPage(p => p - 1)}>Previous options</button><button type="button" disabled={!data || data.length < 100 || loading} onClick={() => setPage(p => p + 1)}>More options</button></div><ErrorMessage error={error} /></div>;
}

export function RecordForm({ fields, initial = {}, editing = false, submit, saved, patientId, resource }: { fields: Field[]; initial?: Record<string, unknown>; editing?: boolean; submit: (value: Record<string, unknown>) => Promise<unknown>; saved: (value: unknown) => void; patientId?: number; resource?: string }) {
  const shown = fields.filter(f => !(editing && f.createOnly));
  const [values, setValues] = useState<Record<string, unknown>>(() => Object.fromEntries(shown.map(f => {
    let value = initial[f.key] ?? (f.type === 'checkbox' ? f.key === 'isActive' : f.options?.[0] ?? (f.key === 'prescriptionDate' ? dateKey() : ''));
    if (f.type === 'datetime' && value) value = localInput(String(value));
    if (f.type === 'time' && value) value = String(value).slice(0, 5);
    return [f.key, value];
  })));
  const [busy, setBusy] = useState(false); const [error, setError] = useState<unknown>();
  function change(key: string, value: unknown) {
    setValues(old => ({ ...old, [key]: value, ...(key === 'role' && value !== 'staff' ? { staffDoctorId: '', staffOrganizationId: '' } : {}) }));
    if (value && key === 'appointmentId' && ['visits', 'followups'].includes(resource || '')) {
      api<DataRow>(`/appointments/${value}`).then(row => setValues(old => ({ ...old, patientId: row.patientId, doctorId: row.doctorId, ...(resource === 'visits' ? { locationId: row.locationId } : {}) }))).catch(setError);
    }
    if (value && key === 'consultationId' && resource === 'prescriptions') {
      api<DataRow>(`/consultations/${value}`).then(row => api<DataRow>(`/visits/${row.visitId}`)).then(row => setValues(old => ({ ...old, patientId: row.patientId, doctorId: row.doctorId }))).catch(setError);
    }
  }
  async function save(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError(undefined);
    try {
      const payload: Record<string, unknown> = {};
      for (const field of shown) {
        let value = values[field.key];
        if (field.type === 'checkbox') value = Boolean(value);
        else if (value === '' || value === undefined) value = null;
        else if (field.relation || field.type === 'number' || field.key === 'dayOfWeek') value = Number(value);
        else if (field.type === 'datetime') value = toUtc(String(value));
        payload[field.key] = value;
      }
      if (resource === 'users' && payload.role === 'staff' && Boolean(payload.staffDoctorId) === Boolean(payload.staffOrganizationId)) throw new Error('Assign staff to exactly one doctor or one organization.');
      if (values.startTime && values.endTime && String(values.endTime) <= String(values.startTime)) throw new Error('End time must be after start time.');
      if (initial.updatedAt) payload.expectedUpdatedAt = initial.updatedAt;
      saved(await submit(payload));
    } catch (e) { setError(e); } finally { setBusy(false); }
  }
  return <form onSubmit={save} className="record-form">
    {shown.filter(f => !f.key.startsWith('staff') || values.role === 'staff').map(field => <div key={field.key} className={field.type === 'textarea' ? 'wide-field' : ''}>
      {field.relation ? <><span className="field-label">{field.label}{field.required ? ' *' : ''}</span><RelationSelect field={field} value={values[field.key]} change={v => change(field.key, v)} patientId={patientId} disabled={busy} /></> : <label>{field.label}{field.required ? ' *' : ''}
        {field.options ? <select aria-label={field.label} required={field.required} value={String(values[field.key] ?? '')} onChange={e => change(field.key, e.target.value)}>{field.options.map(option => <option key={option} value={option}>{field.key === 'dayOfWeek' ? ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][Number(option)] : option.replaceAll('_', ' ')}</option>)}</select>
          : field.type === 'textarea' ? <textarea rows={3} value={String(values[field.key] ?? '')} onChange={e => change(field.key, e.target.value)} />
          : field.type === 'checkbox' ? <input type="checkbox" checked={Boolean(values[field.key])} onChange={e => change(field.key, e.target.checked)} />
          : <input type={field.type === 'datetime' ? 'datetime-local' : field.type || 'text'} required={field.required} min={field.min} step={field.type === 'number' ? field.key === 'amount' ? 'any' : '1' : undefined} autoComplete={field.type === 'password' ? 'new-password' : undefined} value={String(values[field.key] ?? '')} onChange={e => change(field.key, e.target.value)} />}
      </label>}
    </div>)}
    <div className="wide-field"><ErrorMessage error={error} /><button className="primary-button" disabled={busy}>{busy ? 'Saving…' : 'Save'}</button></div>
  </form>;
}
