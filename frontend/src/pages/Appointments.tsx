import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api, query } from '../api/client';
import type { AppointmentDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, Status, useLoad } from '../components/Common';
import { AppointmentActions } from '../features/AppointmentActions';
import { addDays, dateKey, dayStart, displayTime } from '../lib/time';

export function Appointments() {
  const { user } = useAuth(); const [date, setDate] = useState(dateKey()); const [through, setThrough] = useState(addDays(dateKey(), 7));
  const [page, setPage] = useState(1); const [status, setStatus] = useState(''); const [selected, setSelected] = useState<AppointmentDto>();
  const { data, loading, error, reload } = useLoad(() => api<AppointmentDto[]>(`/appointments?${query({ from: dayStart(date), to: dayStart(addDays(through, 1)), status, page, pageSize: 25 })}`), [date, through, status, page]);
  return <section className="panel secondary-page"><div className="section-heading"><h1>Appointments</h1>{user?.role !== 'doctor' && <Link className="primary-button" to="/book">Book appointment</Link>}</div><div className="report-filters"><label>From<input type="date" value={date} onChange={e => { if (e.target.value) { setDate(e.target.value); setPage(1); } }} /></label><label>Through<input type="date" value={through} onChange={e => { if (e.target.value) { setThrough(e.target.value); setPage(1); } }} /></label><label>Status<select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}><option value="">All statuses</option>{['booked', 'arrived', 'waiting', 'in_consultation', 'completed', 'cancelled'].map(s => <option key={s} value={s}>{s.replaceAll('_', ' ')}</option>)}</select></label></div><ErrorMessage error={error} />{loading ? <Loading /> : !data?.length ? <Empty /> : <div className="table-scroll"><table><thead><tr><th>Time · Beirut</th><th>Patient</th><th>Doctor</th><th>Location</th><th>Status</th><th /></tr></thead><tbody>{data.map(a => <tr key={a.id}><td>{displayTime(a.startDatetime)}</td><td>{a.patientName}</td><td>{a.doctorName}</td><td>{a.locationName}</td><td><Status value={a.status} /></td><td><button onClick={() => setSelected(a)}>View appointment</button></td></tr>)}</tbody></table></div>}<div className="pagination"><button disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</button><span>Page {page}</span><button disabled={!data || data.length < 25} onClick={() => setPage(p => p + 1)}>Next</button></div>{selected && <AppointmentActions appointment={selected} changed={reload} close={() => setSelected(undefined)} />}</section>;
}
