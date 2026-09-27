import { Link } from 'react-router-dom';
import { api, pages, query } from '../api/client';
import type { DoctorLocationDto, NotificationDto, ScheduleDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, useLoad } from '../components/Common';

export function DashboardContext() {
  const { user } = useAuth();
  const result = useLoad(async () => {
    const [notifications, schedules, mappings] = await Promise.all([
      api<NotificationDto[]>(`/notifications?${query({ userId: user!.userId, pageSize: 5 })}`),
      user!.role === 'doctor' ? pages<ScheduleDto>('/schedules') : Promise.resolve([] as ScheduleDto[]),
      user!.role === 'doctor' ? pages<DoctorLocationDto>('/doctor-locations') : Promise.resolve([] as DoctorLocationDto[]),
    ]);
    return { notifications, schedules, mappings };
  }, [user!.userId]);
  return <div className="report-grid"><section className="panel"><h2>Your notifications</h2><ErrorMessage error={result.error} />{result.loading ? <Loading /> : !result.data?.notifications.length ? <Empty>No notifications.</Empty> : <ul className="notification-preview">{result.data.notifications.map(n => <li key={n.id}><strong>{n.isRead ? 'Read' : 'Unread'}</strong> · {n.message}</li>)}</ul>}<Link className="outline-button" to="/notifications">View notifications</Link></section>{user!.role === 'doctor' && <section className="panel"><h2>Your working hours</h2>{result.loading ? <Loading /> : !result.data?.schedules.some(s => s.isActive) ? <Empty>No active recurring hours.</Empty> : <div className="table-scroll"><table><thead><tr><th>Location</th><th>Day</th><th>Hours · Beirut</th></tr></thead><tbody>{result.data.schedules.filter(s => s.isActive).map(s => <tr key={s.id}><td>{result.data!.mappings.find(m => m.id === s.doctorLocationId)?.locationName}</td><td>{['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][s.dayOfWeek]}</td><td>{s.startTime.slice(0, 5)} – {s.endTime.slice(0, 5)}</td></tr>)}</tbody></table></div>}<Link className="outline-button" to="/records/schedules">Manage working hours</Link></section>}</div>;
}
