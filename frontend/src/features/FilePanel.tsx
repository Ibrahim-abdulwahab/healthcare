import { useState } from 'react';
import { api, post, put, query, remove } from '../api/client';
import type { HealthcareFileDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { Empty, ErrorMessage, Loading, Modal, useLoad } from '../components/Common';
import { RecordForm } from '../components/RecordForm';
import { canWrite } from '../lib/permissions';

export function FilePanel({ table, recordId, resource }: { table: string; recordId: number; resource: string }) {
  const { user } = useAuth(); const { data, loading, error, reload } = useLoad(() => api<HealthcareFileDto[]>(`/files?${query({ tableName: table, recordId })}`), [table, recordId]);
  const [edit, setEdit] = useState<HealthcareFileDto | 'new'>(); const [deleting, setDeleting] = useState<HealthcareFileDto>(); const [failure, setFailure] = useState<unknown>();
  const writable = canWrite(user!.role, resource);
  return <section className="file-panel"><h3>Attachments</h3><p className="detail-note">Links to existing securely hosted files. Binary upload and storage are not provided by this application.</p><ErrorMessage error={error || failure} />{loading ? <Loading /> : !data?.length ? <Empty>No attachments.</Empty> : <ul>{data.map(file => <li key={file.id}><a href={/^https:\/\//.test(file.fileUrl) ? file.fileUrl : undefined} target="_blank" rel="noopener noreferrer">{file.fileName} ↗</a>{writable && <button onClick={() => setEdit(file)}>Edit link</button>}{user?.role === 'admin' && <button onClick={() => setDeleting(file)}>Remove</button>}</li>)}</ul>}
    {writable && <button className="outline-button" onClick={() => setEdit('new')}>Add attachment link</button>}
    {edit && <Modal title="Attachment link" close={() => setEdit(undefined)}><RecordForm fields={[{ key: 'fileName', label: 'File name', required: true }, { key: 'fileUrl', label: 'Existing HTTPS file URL', required: true }]} initial={edit === 'new' ? {} : { ...edit }} submit={value => edit === 'new' ? post('/files', { ...value, tableName: table, recordId }) : put(`/files/${edit.id}`, { ...value, tableName: table, recordId })} saved={() => { setEdit(undefined); reload(); }} /></Modal>}
    {deleting && <Modal title="Remove attachment link?" close={() => setDeleting(undefined)}><p>The hosted file is not deleted.</p><button className="danger-button" onClick={async () => { try { await remove(`/files/${deleting.id}`); setDeleting(undefined); reload(); } catch (e) { setFailure(e); setDeleting(undefined); } }}>Remove link</button></Modal>}
  </section>;
}
