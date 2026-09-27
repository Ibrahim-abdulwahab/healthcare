import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api, post, put, query, remove } from '../api/client';
import type { DataRow } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, Modal, Status, useLoad } from '../components/Common';
import { RecordForm, rowLabel } from '../components/RecordForm';
import { canDelete, canWrite } from '../lib/permissions';
import { displayTime } from '../lib/time';
import { resources, label } from './resources';
import { FilePanel } from './FilePanel';

function display(row: DataRow, key: string) {
  if (key === 'status' || key === 'paymentStatus') return <Status value={String(row[key])} />;
  if (key === 'dayOfWeek') return ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][Number(row[key])];
  if (key === 'isAvailable') return row[key] ? 'Extra hours' : 'Unavailable';
  if (key.endsWith('Id') && row[key.slice(0, -2) + 'Name']) return String(row[key.slice(0, -2) + 'Name']);
  if (key === 'createdAt' || key === 'checkInTime' || key === 'checkOutTime') return displayTime(row[key] as string | null);
  if (typeof row[key] === 'boolean') return row[key] ? 'Yes' : 'No';
  return row[key] == null || row[key] === '' ? '—' : String(row[key]);
}

export function ResourcePage({ resource, patientId, embedded = false }: { resource: string; patientId?: number; embedded?: boolean }) {
  const config = resources[resource]; const { user } = useAuth(); const [params] = useSearchParams();
  const [search, setSearch] = useState(''); const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<DataRow | 'new'>(); const [detail, setDetail] = useState<DataRow>(); const [deleting, setDeleting] = useState<DataRow>();
  const [failure, setFailure] = useState<unknown>(); const [notice, setNotice] = useState('');
  const prescriptionId = params.get('prescriptionId');
  const { data, loading, error, reload } = useLoad(() => api<DataRow[]>(`/${resource}?${query({ search, page, pageSize: 25, patientId, prescriptionId })}`), [resource, patientId, prescriptionId, search, page]);
  if (!config || (config.admin && user?.role !== 'admin')) return <ErrorMessage error="You do not have access to this page." />;
  const initial = editing && editing !== 'new' ? editing : { patientId, prescriptionId: prescriptionId ? Number(prescriptionId) : undefined, ...(user?.role === 'doctor' ? { doctorId: user.userId } : {}), ...(params.get('appointmentId') ? { appointmentId: Number(params.get('appointmentId')) } : {}) };
  return <section className={embedded ? 'resource-section' : 'panel secondary-page'}>
    <div className="section-heading"><div><h2>{config.title}</h2>{resource === 'paymentrecords' && <p>Recorded collections. No invoice balances are calculated.</p>}{resource === 'schedule-exceptions' && <p>Extra hours add working time. Unavailable periods subtract time.</p>}{resource === 'schedules' && <p>Start intervals determine bookable start times; visit types determine appointment length.</p>}</div>{!config.readOnly && canWrite(user!.role, resource, true) && <button className="primary-button" onClick={() => setEditing('new')}>Add {config.singular}</button>}</div>
    <label className="search-field">Search<input placeholder={`Search ${config.title.toLowerCase()}…`} value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></label>
    <ErrorMessage error={error || failure} />{notice && <p role="status" className="success-banner">{notice}</p>}
    {loading ? <Loading /> : !data?.length ? <Empty /> : <div className="table-scroll"><table><thead><tr>{config.columns.map(key => <th key={key}>{config.fields.find(f => f.key === key)?.label || label(key)}</th>)}<th>Actions</th></tr></thead><tbody>{data.map(row => <tr key={row.id}>{config.columns.map(key => <td key={key}>{display(row, key)}</td>)}<td className="row-actions"><button onClick={() => setDetail(row)}>View</button>{resource === 'patients' && <Link to={`/patients/${row.id}`}>Open chart</Link>}{resource === 'prescriptions' && <Link to={`/records/prescriptionitems?prescriptionId=${row.id}`}>Items</Link>}{!config.readOnly && canWrite(user!.role, resource) && <button onClick={() => setEditing(row)}>Edit</button>}{!config.readOnly && canDelete(user!.role, resource) && <button className="text-danger" onClick={() => setDeleting(row)}>Delete</button>}</td></tr>)}</tbody></table></div>}
    <div className="pagination"><button disabled={page === 1 || loading} onClick={() => setPage(p => p - 1)}>Previous</button><span>Page {page}</span><button disabled={loading || !data || data.length < 25} onClick={() => setPage(p => p + 1)}>Next</button></div>
    {editing && <Modal title={`${editing === 'new' ? 'Add' : 'Edit'} ${config.singular}`} close={() => setEditing(undefined)}><RecordForm resource={resource} fields={config.fields} initial={initial} editing={editing !== 'new'} patientId={patientId} submit={value => editing === 'new' ? post(`/${resource}`, value) : put(`/${resource}/${editing.id}`, value)} saved={result => { setEditing(undefined); reload(); setNotice('Saved.'); if (resource === 'patients') setDetail(result as DataRow); }} /></Modal>}
    {detail && <Modal title={`${config.singular[0].toUpperCase() + config.singular.slice(1)} details`} close={() => setDetail(undefined)}><dl className="record-details">{Object.entries(detail).filter(([key]) => key !== 'id').map(([key]) => <div key={key}><dt>{config.fields.find(f => f.key === key)?.label || label(key)}</dt><dd>{display(detail, key)}</dd></div>)}</dl>{resource === 'patients' && user?.role !== 'doctor' && <Link className="primary-button" to={`/book?patientId=${detail.id}&patientName=${encodeURIComponent(rowLabel(detail))}`}>Book appointment</Link>}{config.fileTable && <FilePanel table={config.fileTable} recordId={detail.id} resource={resource} />}</Modal>}
    {deleting && <Modal title={`Delete ${config.singular}?`} close={() => setDeleting(undefined)}><p>This permanently removes this record. Linked records may prevent deletion.</p><ErrorMessage error={failure} /><button className="danger-button" onClick={async () => { try { await remove(`/${resource}/${deleting.id}`); setDeleting(undefined); setFailure(undefined); reload(); } catch (e) { setFailure(e); } }}>Confirm delete</button></Modal>}
  </section>;
}
