import { useState } from 'react';
import { api, patch, query } from '../api/client';
import type { NotificationDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, useLoad } from '../components/Common';
import { displayTime } from '../lib/time';

export function Notifications() {
  const { user } = useAuth(); const [page, setPage] = useState(1); const [failure, setFailure] = useState<unknown>();
  const { data, loading, error, reload } = useLoad(() => api<NotificationDto[]>(`/notifications?${query({ userId: user?.userId, page, pageSize: 25 })}`), [user?.userId, page]);
  return <section className="panel secondary-page"><div className="section-heading"><h1>Notifications</h1><button className="outline-button" onClick={reload}>Refresh</button></div><ErrorMessage error={error || failure} />{loading ? <Loading /> : !data?.length ? <Empty>You’re all caught up.</Empty> : <ul className="notification-list">{data.map(n => <li key={n.id} className={n.isRead ? '' : 'unread'}><div><strong>{n.isRead ? 'Read' : 'Unread'} · {n.type}</strong><p>{n.message}</p><small>{displayTime(n.createdAt)}</small></div>{!n.isRead && <button className="outline-button" onClick={async () => { try { await patch(`/notifications/${n.id}/read`, {}); reload(); } catch (e) { setFailure(e); } }}>Mark as read</button>}</li>)}</ul>}<div className="pagination"><button disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</button><span>Page {page}</span><button disabled={!data || data.length < 25} onClick={() => setPage(p => p + 1)}>Next</button></div></section>;
}
