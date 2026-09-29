import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api/client';
import type { PatientDto } from '../api/types';
import { useAuth } from '../auth/Auth';
import { ErrorMessage, Loading, useLoad } from '../components/Common';
import { clinicalResources, resources } from '../features/resources';
import { ResourcePage } from '../features/ResourcePage';
import { FilePanel } from '../features/FilePanel';
import { RelationSelect } from '../components/RecordForm';

export function PatientChart() {
  const { id } = useParams(); const { user } = useAuth(); const [tab, setTab] = useState('visits');
  const [visitId, setVisitId] = useState<number>();
  useEffect(() => { setVisitId(undefined); }, [id]);
  const { data: patient, loading, error } = useLoad(() => api<PatientDto>(`/patients/${id}`), [id]);
  if (loading) return <Loading />; if (error || !patient) return <ErrorMessage error={error || 'Patient not found.'} />;
  return <><section className="panel patient-header"><div><p className="eyebrow">PATIENT CHART · {patient.mrn}</p><h1>{patient.firstName} {patient.lastName}</h1><p>{patient.dateOfBirth || 'Date of birth not recorded'} · {patient.phone || 'No phone'} · {patient.email || 'No email'}</p><p>{patient.address}</p><p className="detail-note">Clinical records below are limited to your authorized doctor scope.</p></div>{user?.role !== 'doctor' && <Link className="primary-button" to={`/book?patientId=${id}&patientName=${encodeURIComponent(patient.firstName + ' ' + patient.lastName)}`}>Book appointment</Link>}</section><section className="panel"><span className="field-label">Visit context (optional)</span><RelationSelect field={{ key: 'visitId', label: 'Visit context', relation: 'visits' }} value={visitId} change={value => setVisitId(Number(value) || undefined)} patientId={Number(id)} /><p className="detail-note">Select a visit to prefill clinical forms. Without a selection, a visit is preselected only when exactly one is available.</p></section><div className="tabs" role="tablist">{clinicalResources.map(key => <button role="tab" aria-selected={tab === key} key={key} onClick={() => setTab(key)}>{resources[key].title}</button>)}</div><ResourcePage key={tab} resource={tab} patientId={Number(id)} visitId={visitId} /><section className="panel"><FilePanel table="patients" recordId={Number(id)} resource="patients" /></section></>;
}
