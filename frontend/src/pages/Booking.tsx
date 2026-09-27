import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { api, pages, post, put, query } from '../api/client';
import type { AppointmentDto, AvailableSlotDto, DoctorDto, DoctorLocationDto, LocationDto, PatientDto, VisitTypeDto } from '../api/types';
import { Empty, ErrorMessage, Loading, Modal, useLoad } from '../components/Common';
import { RecordForm } from '../components/RecordForm';
import { resources } from '../features/resources';
import { clockTime, dateKey } from '../lib/time';

export function Booking({ readOnly = false }: { readOnly?: boolean }) {
  const [params] = useSearchParams(); const navigate = useNavigate(); const appointmentId = params.get('appointmentId');
  const [patientId, setPatientId] = useState(Number(params.get('patientId')) || 0); const [patientName, setPatientName] = useState(params.get('patientName') || '');
  const [doctorId, setDoctorId] = useState(0); const [locationId, setLocationId] = useState(0); const [visitTypeId, setVisitTypeId] = useState(0);
  const [date, setDate] = useState(dateKey()); const [start, setStart] = useState(''); const [notes, setNotes] = useState('');
  const [search, setSearch] = useState(''); const [newPatient, setNewPatient] = useState(false); const [busy, setBusy] = useState(false); const [failure, setFailure] = useState<unknown>();
  const [original, setOriginal] = useState<AppointmentDto>();
  const options = useLoad(async () => {
    const [doctors, locations, mappings, types] = await Promise.all([pages<DoctorDto>('/doctors'), pages<LocationDto>('/locations'), pages<DoctorLocationDto>('/doctor-locations'), pages<VisitTypeDto>('/visit-types')]);
    return { doctors: doctors.filter(d => d.isActive), locations: locations.filter(l => l.isActive), mappings: mappings.filter(m => m.isActive), types: types.filter(t => t.isActive) };
  }, []);
  const patients = useLoad(() => api<PatientDto[]>(`/patients?${query({ search, pageSize: 25 })}`), [search]);
  useEffect(() => {
    if (!appointmentId) return;
    api<AppointmentDto>(`/appointments/${appointmentId}`).then(a => {
      if (a.hasVisits || ['completed', 'cancelled'].includes(a.status)) throw new Error('This appointment cannot be rescheduled.');
      setOriginal(a); setPatientId(a.patientId); setPatientName(a.patientName); setDoctorId(a.doctorId); setLocationId(a.locationId); setVisitTypeId(a.visitTypeId); setDate(dateKey(new Date(a.startDatetime))); setNotes(a.notes || '');
    }).catch(setFailure);
  }, [appointmentId]);
  useEffect(() => { if (options.data && !appointmentId) { if (options.data.doctors.length === 1) setDoctorId(options.data.doctors[0].id); if (options.data.types.length === 1) setVisitTypeId(options.data.types[0].id); } }, [options.data, appointmentId]);
  const slots = useLoad(async () => {
    setStart('');
    if (!doctorId || !locationId || !visitTypeId) return [];
    return api<AvailableSlotDto[]>(`${appointmentId ? `/appointments/${appointmentId}/availability` : '/availability'}?${query({ doctorId, locationId, visitTypeId, date })}`);
  }, [doctorId, locationId, visitTypeId, date, appointmentId]);
  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setFailure(undefined);
    try {
      if (!start || !patientId) throw new Error('Select a patient and an available start time.');
      const value = { patientId, doctorId, locationId, visitTypeId, startDatetime: start, notes, expectedUpdatedAt: original?.updatedAt };
      if (appointmentId) await put(`/appointments/${appointmentId}/reschedule`, value); else await post('/appointments', value);
      navigate(`/calendar?date=${date}&doctorId=${doctorId}`, { state: { notice: appointmentId ? 'Appointment rescheduled.' : 'Appointment booked.' } });
    } catch (error) { setFailure(error); slots.reload(); } finally { setBusy(false); }
  }
  if (options.loading) return <Loading />;
  return <section className="panel booking-page"><div className="section-heading"><div><p className="eyebrow">APPOINTMENTS</p><h1>{readOnly ? 'Find availability' : appointmentId ? 'Reschedule appointment' : 'Book an appointment'}</h1><p>Choose an available time. Duration comes from the selected visit type.</p></div><Link to="/calendar">Back to calendar</Link></div><ErrorMessage error={options.error || failure} />
    <form onSubmit={submit}>
      {!readOnly && (<fieldset disabled={!!appointmentId}><legend>1. Patient</legend>{patientId > 0 && <p className="selected-patient">Selected: <strong>{patientName || `Patient ${patientId}`}</strong></p>}<div className="inline-controls"><label>Find patient<input value={search} placeholder="Name, MRN or phone" onChange={e => setSearch(e.target.value)} /></label><button type="button" className="outline-button" onClick={() => setNewPatient(true)}>New patient</button></div><ErrorMessage error={patients.error} /><select aria-label="Patient" value={patientId || ''} onChange={e => { const id = Number(e.target.value); const p = patients.data?.find(p => p.id === id); setPatientId(id); setPatientName(p ? `${p.firstName} ${p.lastName}` : ''); }}><option value="">Select patient</option>{patientId > 0 && !patients.data?.some(p => p.id === patientId) && <option value={patientId}>{patientName}</option>}{patients.data?.map(p => <option key={p.id} value={p.id}>{p.firstName} {p.lastName} · {p.mrn}</option>)}</select><p className="detail-note">Search results contain patients in your assigned scope. A newly registered patient becomes visible after booking.</p></fieldset>)}
      <fieldset><legend>2. Doctor and location</legend><div className="form-row"><label>Doctor<select aria-label="Doctor" required value={doctorId || ''} onChange={e => { setDoctorId(Number(e.target.value)); setLocationId(0); }}><option value="">Select doctor</option>{options.data?.doctors.map(d => <option key={d.id} value={d.id}>Dr. {d.firstName} {d.lastName}</option>)}</select></label><label>Location<select aria-label="Location" required value={locationId || ''} onChange={e => setLocationId(Number(e.target.value))}><option value="">Select location</option>{options.data?.locations.filter(l => options.data!.mappings.some(m => m.doctorId === doctorId && m.locationId === l.id)).map(l => <option key={l.id} value={l.id}>{l.name}</option>)}</select></label></div></fieldset>
      <fieldset><legend>3. Visit and time · Asia/Beirut</legend><div className="form-row"><label>Visit type<select aria-label="Visit type" required value={visitTypeId || ''} onChange={e => setVisitTypeId(Number(e.target.value))}><option value="">Select visit type</option>{options.data?.types.map(t => <option key={t.id} value={t.id}>{t.name} · {t.defaultDuration} min</option>)}</select></label><label>Date<input type="date" required value={date} onChange={e => setDate(e.target.value)} /></label></div><ErrorMessage error={slots.error} />{slots.loading ? <Loading /> : !slots.data?.length ? <Empty>{doctorId && locationId && visitTypeId ? 'No available appointments on this date.' : 'Choose a doctor, location and visit type to see available times.'}</Empty> : <div className="slot-options" role="group" aria-label="Available times">{slots.data.map(slot => <button type="button" aria-pressed={start === slot.startDatetime} key={slot.startDatetime} onClick={() => setStart(slot.startDatetime)}>{clockTime(slot.startDatetime)} – {clockTime(slot.endDatetime)}</button>)}</div>}</fieldset>
      {!readOnly && !appointmentId && <label>Booking note (optional)<textarea value={notes} onChange={e => setNotes(e.target.value)} /></label>}{!readOnly && <button className="primary-button" disabled={busy || !start || !patientId || (!!appointmentId && !original)}>{busy ? 'Saving…' : appointmentId ? 'Confirm reschedule' : 'Book appointment'}</button>}
    </form>{newPatient && <Modal title="New patient" close={() => setNewPatient(false)}><RecordForm fields={resources.patients.fields} submit={value => post<PatientDto>('/patients', value)} saved={value => { const p = value as PatientDto; setPatientId(p.id); setPatientName(`${p.firstName} ${p.lastName}`); setNewPatient(false); patients.reload(); }} /></Modal>}
  </section>;
}
