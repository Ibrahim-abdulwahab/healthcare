import { useEffect, useState, type DragEvent } from 'react';
import { Link, useLocation, useSearchParams } from 'react-router-dom';
import { pages, put, query } from '../api/client';
import type { AppointmentDto, DoctorDto, DoctorLocationDto, LocationDto, ScheduleDto, ScheduleExceptionDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, useLoad } from '../components/Common';
import { AppointmentActions } from '../features/AppointmentActions';
import { canReschedule } from '../lib/permissions';
import { addDays, clockTime, dateKey, dayStart, minutes, monday, shortDate, toUtc, weekday } from '../lib/time';

const colors = ['rose', 'blue', 'green'];
const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
export function Calendar() {
  const { user } = useAuth(); const [params] = useSearchParams(); const route = useLocation();
  const [date, setDate] = useState(params.get('date') || dateKey()); const [view, setView] = useState('week');
  const [doctorId, setDoctorId] = useState(Number(params.get('doctorId')) || (user?.role === 'doctor' ? user.userId : 0));
  const [locationId, setLocationId] = useState(0); const [selected, setSelected] = useState<AppointmentDto>(); const [pending, setPending] = useState<number>();
  const [message, setMessage] = useState((route.state as { notice?: string } | null)?.notice || ''); const [failure, setFailure] = useState<unknown>();
  const first = view === 'week' ? monday(date) : date; const dates = Array.from({ length: view === 'week' ? 7 : 1 }, (_, i) => addDays(first, i));
  const result = useLoad(async () => {
    const [appointments, doctors, locations, mappings, schedules, exceptions] = await Promise.all([
      pages<AppointmentDto>(`/appointments?${query({ from: dayStart(first), to: dayStart(addDays(first, dates.length)), doctorId: doctorId || undefined, locationId: locationId || undefined })}`),
      pages<DoctorDto>('/doctors'), pages<LocationDto>('/locations'), pages<DoctorLocationDto>('/doctor-locations'), pages<ScheduleDto>('/schedules'), pages<ScheduleExceptionDto>('/schedule-exceptions'),
    ]);
    return { appointments, doctors, locations, mappings, schedules, exceptions };
  }, [first, view, doctorId, locationId]);
  useEffect(() => { if (!doctorId && result.data?.doctors.length) setDoctorId(result.data.doctors[0].id); }, [result.data, doctorId]);
  const data = result.data;
  const doctor = data?.doctors.find(d => d.id === doctorId);
  const mappingIds = data?.mappings.filter(m => m.doctorId === doctorId && (!locationId || m.locationId === locationId)).map(m => m.id) || [];
  const schedules = data?.schedules.filter(s => s.isActive && mappingIds.includes(s.doctorLocationId)) || [];
  function mappingLocation(id: number) { return data?.locations.find(l => l.id === data?.mappings.find(m => m.id === id)?.locationId); }
  function color(id: number) { return colors[Math.max(0, data?.locations.findIndex(l => l.id === id) ?? 0) % colors.length]; }
  const visibleEvents = data?.appointments.filter(a => a.doctorId === doctorId) || [];
  const visibleExceptions = data?.exceptions.filter(e => dates.includes(e.date) && mappingIds.includes(e.doctorLocationId)) || [];
  const starts = [...visibleExceptions.map(e => minutes(e.startTime)), ...schedules.map(s => minutes(s.startTime)), ...visibleEvents.map(a => minutes(clockTime(a.startDatetime)))];
  const ends = [...visibleExceptions.map(e => minutes(e.endTime)), ...schedules.map(s => minutes(s.endTime)), ...visibleEvents.map(a => minutes(clockTime(a.endDatetime)))];
  const minHour = Math.min(7, Math.floor(Math.min(...starts, 420) / 60));
  const maxHour = Math.max(19, Math.ceil(Math.max(...ends, 1140) / 60));
  const height = (maxHour - minHour) * 70;
  const top = (time: string) => (minutes(time) - minHour * 60) * 70 / 60;
  async function drop(event: DragEvent<HTMLDivElement>, targetDate: string) {
    event.preventDefault(); const id = Number(event.dataTransfer.getData('text/plain')); const appointment = visibleEvents.find(a => a.id === id);
    if (!appointment || !canReschedule(user!.role, appointment) || pending) return;
    const rawMinute = minHour * 60 + (event.clientY - event.currentTarget.getBoundingClientRect().top) / 70 * 60;
    const mapping = data?.mappings.find(m => m.doctorId === doctorId && m.locationId === appointment.locationId);
    const applicable = schedules.find(s => s.doctorLocationId === mapping?.id && s.dayOfWeek === weekday(targetDate) && rawMinute >= minutes(s.startTime) && rawMinute < minutes(s.endTime));
    const step = applicable?.slotDuration || 5; const anchor = applicable ? minutes(applicable.startTime) : 0;
    const minute = Math.max(0, Math.min(1439, anchor + Math.round((rawMinute - anchor) / step) * step));
    setPending(id); setFailure(undefined); setMessage('');
    try {
      const startDatetime = toUtc(`${targetDate}T${String(Math.floor(minute / 60)).padStart(2, '0')}:${String(minute % 60).padStart(2, '0')}`);
      await put(`/appointments/${id}/reschedule`, { doctorId: appointment.doctorId, locationId: appointment.locationId, visitTypeId: appointment.visitTypeId, startDatetime, expectedUpdatedAt: appointment.updatedAt });
      setMessage('Appointment rescheduled.'); result.reload();
    } catch (error) { setFailure(error); setMessage('The appointment remains at its original time.'); } finally { setPending(undefined); }
  }
  return <><div className="page-heading"><div><h1>{doctor ? `Dr. ${doctor.firstName} ${doctor.lastName}` : 'Practice'} – {view === 'week' ? 'Weekly' : 'Daily'} Schedule</h1><p>Multiple locations, one calendar</p></div><div className="calendar-controls"><select aria-label="Calendar view" value={view} onChange={e => setView(e.target.value)}><option value="week">Week</option><option value="day">Day</option></select><div className="arrows"><button className="icon-button" aria-label="Previous period" onClick={() => setDate(addDays(date, view === 'week' ? -7 : -1))}>‹</button><button className="icon-button" aria-label="Next period" onClick={() => setDate(addDays(date, view === 'week' ? 7 : 1))}>›</button></div><input type="date" aria-label="Calendar date" value={date} onChange={e => setDate(e.target.value)} /><button className="today-button" onClick={() => setDate(dateKey())}>Today</button></div></div>
    <div className="calendar-filters"><label>Doctor<select value={doctorId} onChange={e => { setDoctorId(Number(e.target.value)); setLocationId(0); }}>{data?.doctors.map(d => <option key={d.id} value={d.id}>Dr. {d.firstName} {d.lastName}</option>)}</select></label><label>Location<select value={locationId} onChange={e => setLocationId(Number(e.target.value))}><option value="0">All assigned locations</option>{data?.locations.filter(l => data.mappings.some(m => m.doctorId === doctorId && m.locationId === l.id)).map(l => <option key={l.id} value={l.id}>{l.name}</option>)}</select></label>{user?.role !== 'doctor' && <Link className="primary-button" to="/book">Book appointment</Link>}<span className="detail-note">Asia/Beirut · {shortDate(first)} – {shortDate(dates.at(-1)!)}</span></div>
    <ErrorMessage error={result.error || failure} />{message && <p className="success-banner" role="status">{message}</p>}{result.loading && <Loading />}
    <section className="schedule-layout"><div className="calendar-scroll"><div className={`calendar ${view === 'day' ? 'day-view' : ''}`}><div className="day-heading time-heading" />{dates.map(d => <div key={d} className={`day-heading ${d === dateKey() ? 'is-today' : ''}`}><strong>{days[weekday(d)]}</strong><span>{shortDate(d)}</span></div>)}<div className="time-axis" style={{ height }}>{Array.from({ length: maxHour - minHour + 1 }, (_, i) => <span key={i} className="time-label" style={{ top: i * 70 }}>{String(minHour + i).padStart(2, '0')}:00</span>)}</div>
      {dates.map(d => {
        const sessions = schedules.filter(s => s.dayOfWeek === weekday(d)); const exceptions = data?.exceptions.filter(e => e.date === d && mappingIds.includes(e.doctorLocationId)) || [];
        const appointments = visibleEvents.filter(a => dateKey(new Date(a.startDatetime)) === d);
        return <div className={`day-column ${[0, 6].includes(weekday(d)) ? 'weekend' : ''}`} key={d} style={{ height }} onDragOver={e => { if (user?.role !== 'doctor') e.preventDefault(); }} onDrop={e => drop(e, d)} aria-label={`${days[weekday(d)]} ${d}`}>
          {sessions.map(s => { const location = mappingLocation(s.doctorLocationId); return <div key={s.id} className={`working-block ${color(location?.id || 0)}`} style={{ top: top(s.startTime), height: (minutes(s.endTime) - minutes(s.startTime)) * 70 / 60 }}><strong>✚ {location?.name}</strong><small>{s.startTime.slice(0, 5)}–{s.endTime.slice(0, 5)}</small></div>; })}
          {exceptions.map(e => <div key={`exception-${e.id}`} className={`exception-block ${e.isAvailable ? 'extra-hours' : 'blocked-time'}`} style={{ top: top(e.startTime), height: (minutes(e.endTime) - minutes(e.startTime)) * 70 / 60 }} title={e.reason || ''}>{e.isAvailable ? 'Extra hours' : 'Unavailable'} · {mappingLocation(e.doctorLocationId)?.name}</div>)}
          {appointments.map(a => <button key={a.id} className={`calendar-event ${color(a.locationId)} ${a.status === 'cancelled' ? 'cancelled-event' : ''} ${pending === a.id ? 'pending-event' : ''}`} style={{ top: top(clockTime(a.startDatetime)), minHeight: Math.max(24, (new Date(a.endDatetime).getTime() - new Date(a.startDatetime).getTime()) / 60000 * 70 / 60 - 2) }} draggable={!pending && canReschedule(user!.role, a)} onDragStart={e => e.dataTransfer.setData('text/plain', String(a.id))} onClick={() => setSelected(a)} title={`${a.patientName} · ${a.locationName} · ${a.status.replaceAll('_', ' ')}`}><time>{clockTime(a.startDatetime)}</time><strong>{a.patientName}</strong><small>{a.locationName} {"\u00b7"} {a.status.replaceAll('_', ' ')}</small></button>)}
          {!sessions.length && !exceptions.length && !appointments.length && <div className="empty-day"><span className="empty-calendar-icon">▦</span><p>No working hours<br />scheduled</p></div>}
        </div>;
      })}</div></div><aside className="schedule-sidebar"><section className="panel"><h2>Working Hours (Recurring)</h2>{!schedules.length && <Empty>No recurring hours.</Empty>}{schedules.map(s => { const location = mappingLocation(s.doctorLocationId); return <article key={s.id} className={`hours-card ${color(location?.id || 0)}`}><span className="location-dot" /><span className="location-icon">✚</span><div><strong>{location?.name}</strong><p>{days[s.dayOfWeek]}<br />{s.startTime.slice(0, 5)} – {s.endTime.slice(0, 5)}</p></div></article>; })}{user?.role !== 'staff' && <Link className="outline-button full" to="/records/schedules">＋ Manage Working Hours</Link>}</section><section className="panel legend"><h2>Legend</h2>{data?.locations.filter(l => data.mappings.some(m => m.doctorId === doctorId && m.locationId === l.id)).map(l => <p key={l.id}><i className={`swatch ${color(l.id)}`} />{l.name}{l.city ? ` (${l.city})` : ''}</p>)}<p><i className="swatch blocked-swatch" />Unavailable time</p><p>Cancelled appointments stay visible and do not block booking.</p></section><section className="exception-panel"><p>Working at a different location on a specific day?</p><p>Available exceptions add extra hours. Unavailable exceptions remove time.</p>{user?.role !== 'staff' && <Link className="outline-button full" to="/records/schedule-exceptions">＋ Manage Exceptions</Link>}</section><p className="detail-note">{user?.role === 'doctor' ? 'Appointments are read-only.' : 'Drag an appointment to move it. Select Reschedule for keyboard access or to change location.'}</p></aside></section>
    {selected && <AppointmentActions appointment={selected} close={() => setSelected(undefined)} changed={result.reload} />}
  </>;
}
