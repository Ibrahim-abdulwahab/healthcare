import { Link } from 'react-router-dom';
import { resources } from '../features/resources';

export function Admin() {
  return <><div className="page-heading"><div><p className="eyebrow">ADMINISTRATION</p><h1>Practice management</h1><p>Manage accounts, locations, reference data, and system configuration.</p></div></div><div className="admin-grid">{Object.entries(resources).filter(([, resource]) => resource.admin).map(([key, resource]) => <Link key={key} className="panel admin-card" to={`/records/${key}`}><h2>{resource.title}</h2><p>{resource.readOnly ? 'Inspect backend-generated history' : `Manage ${resource.title.toLowerCase()}`}</p><span>Open →</span></Link>)}</div></>;
}
