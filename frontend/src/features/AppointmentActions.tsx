import { useState } from 'react';
import { Link } from 'react-router-dom';
import { patch, put, remove } from '../api/client';
import type { AppointmentDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { ErrorMessage, Modal, Status } from '../components/Common';
import { canReschedule, nextStatus } from '../lib/permissions';
import { displayTime } from '../lib/time';

export function AppointmentActions({ appointment, changed, close }: { appointment: AppointmentDto; changed: () => void; close: () => void }) {
  const { user } = useAuth(); const [error, setError] = useState<unknown>(); const [busy, setBusy] = useState(false); const [cancel, setCancel] = useState(false);
  const [editingNote, setEditingNote] = useState(false); const [note, setNote] = useState(appointment.notes || ''); const [deleting, setDeleting] = useState(false);
  async function saveNote() {
    setBusy(true); setError(undefined);
    try { await put(`/appointments/${appointment.id}`, { ...appointment, notes: note, expectedUpdatedAt: appointment.updatedAt }); changed(); close(); }
    catch (e) { setError(e); } finally { setBusy(false); }
  }
  async function transition(status: string) {
    setBusy(true); setError(undefined);
    try { await patch(`/appointments/${appointment.id}/status`, { status, expectedUpdatedAt: appointment.updatedAt }); changed(); close(); }
    catch (e) { setError(e); } finally { setBusy(false); }
  }
  return <Modal title={appointment.patientName || 'Appointment'} close={close}><Status value={appointment.status} /><dl className="detail-list"><dt>Doctor</dt><dd>{appointment.doctorName}</dd><dt>Location</dt><dd>{appointment.locationName}</dd><dt>Start</dt><dd>{displayTime(appointment.startDatetime)}</dd><dt>End</dt><dd>{displayTime(appointment.endDatetime)}</dd><dt>Visit type</dt><dd>{appointment.visitTypeName}</dd><dt>Note</dt><dd>{appointment.notes || '—'}</dd></dl><ErrorMessage error={error} /><div className="action-stack"><Link className="outline-button" to={`/patients/${appointment.patientId}`}>Open patient chart</Link>{user?.role !== 'doctor' && !['completed', 'cancelled'].includes(appointment.status) && <><Link className="outline-button" to={`/records/visits?appointmentId=${appointment.id}`}>Check-in / visits</Link>{canReschedule(user!.role, appointment) && <Link className="outline-button" to={`/book?appointmentId=${appointment.id}`}>Reschedule</Link>}{nextStatus[appointment.status] && <button className="primary-button" disabled={busy} onClick={() => transition(nextStatus[appointment.status]!)}>Mark {nextStatus[appointment.status]!.replaceAll('_', ' ')}</button>}{cancel ? <button className="danger-button" disabled={busy} onClick={() => transition('cancelled')}>Confirm cancellation</button> : <button className="danger-button" onClick={() => setCancel(true)}>Cancel appointment</button>}</>}{user?.role !== 'doctor' && (editingNote ? <><label>Booking note<textarea value={note} onChange={e => setNote(e.target.value)} /></label><button className="outline-button" disabled={busy} onClick={saveNote}>Save note</button></> : <button className="outline-button" onClick={() => setEditingNote(true)}>Edit booking note</button>)}{user?.role === 'admin' && (deleting ? <><p>Delete this appointment permanently? Linked clinical records may prevent deletion.</p><button className="danger-button" disabled={busy} onClick={async () => { setBusy(true); try { await remove(`/appointments/${appointment.id}`); changed(); close(); } catch (e) { setError(e); } finally { setBusy(false); } }}>Confirm permanent deletion</button></> : <button className="danger-button" onClick={() => setDeleting(true)}>Delete appointment</button>)}{appointment.hasVisits && <p className="detail-note">Rescheduling is locked because this appointment has visits.</p>}</div></Modal>;
}
