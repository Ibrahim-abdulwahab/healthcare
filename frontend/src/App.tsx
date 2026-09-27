import { Navigate, Outlet, Route, Routes, useLocation, useParams } from 'react-router-dom';
import { useAuth } from './auth/Auth';
import { ErrorMessage, Loading } from './components/Common';
import { Shell } from './layout/Shell';
import { Login } from './pages/Login';
import { Calendar } from './pages/Calendar';
import { Reports } from './pages/Reports';
import { Admin } from './pages/Admin';
import { Notifications } from './pages/Notifications';
import { PatientChart } from './pages/PatientChart';
import { Appointments } from './pages/Appointments';
import { Booking } from './pages/Booking';
import { ResourcePage } from './features/ResourcePage';
import { resources } from './features/resources';
import type { Role } from './lib/permissions';

function Protected({ roles }: { roles?: Role[] }) {
  const auth = useAuth(); const location = useLocation();
  if (auth.loading) return <Loading />;
  if (auth.error) return <main><ErrorMessage error={auth.error} /><button onClick={auth.retry}>Retry session check</button><button onClick={auth.logout}>Sign out</button></main>;
  if (!auth.user) return <Navigate to="/login" state={{ from: location.pathname + location.search }} replace />;
  if (roles && !roles.includes(auth.user.role)) return <ErrorMessage error="You do not have permission to open this page." />;
  return <Outlet />;
}
function ResourceRoute() { const { resource = '' } = useParams(); const { user } = useAuth(); if (!resources[resource]) return <ErrorMessage error="Page not found." />; if (resources[resource].admin && user?.role !== 'admin') return <ErrorMessage error="You do not have permission to open this page." />; return <ResourcePage key={resource} resource={resource} />; }
export function App() {
  return <Routes><Route path="/login" element={<Login />} /><Route element={<Protected />}><Route element={<Shell />}><Route index element={<Reports dashboard />} /><Route path="calendar" element={<Calendar />} /><Route path="availability" element={<Booking readOnly />} /><Route path="appointments" element={<Appointments />} /><Route path="patients" element={<ResourcePage resource="patients" />} /><Route path="patients/:id" element={<PatientChart />} /><Route path="records/:resource" element={<ResourceRoute />} /><Route path="reports" element={<Reports />} /><Route path="notifications" element={<Notifications />} /><Route element={<Protected roles={['admin', 'staff']} />}><Route path="book" element={<Booking />} /></Route><Route element={<Protected roles={['admin']} />}><Route path="admin" element={<Admin />} /></Route><Route path="*" element={<ErrorMessage error="Page not found." />} /></Route></Route></Routes>;
}
