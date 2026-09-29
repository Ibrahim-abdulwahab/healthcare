import { useEffect } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { useAuth, SessionWarning } from '../auth/Auth';
import { Icon } from '../components/Icon';

export function Shell() {
  const { user, logout } = useAuth();
  const route = useLocation();
  useEffect(() => { window.scrollTo(0, 0); }, [route.pathname]);
  const links = [['/', 'dashboard', 'Dashboard'], ['/calendar', 'calendar', 'Calendar'], ['/appointments', 'appointments', 'Appointments'], ['/patients', 'patients', 'Patients'], ['/records/visits', 'medical', 'Visits'], ['/records/paymentrecords', 'payments', 'Payments'], ['/availability', 'clock', 'Availability'], ['/reports', 'reports', 'Reports'], ['/notifications', 'bell', 'Notifications']];
  if (user?.role !== 'staff') links.push(['/records/schedules', 'clock', 'Working hours'], ['/records/schedule-exceptions', 'calendar', 'Exceptions']);
  if (user?.role === 'admin') links.push(['/admin', 'settings', 'Administration']);
  return <><aside className="sidebar"><NavLink className="brand" to="/"><span className="brand-mark">∞</span><span>OmniSync</span></NavLink><p className="tagline">One Doctor. All Locations. In Sync.</p><nav aria-label="Main navigation">{links.map(([to, icon, name]) => <NavLink key={to} to={to} end={to === '/'} className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} title={name}><span className="nav-icon" aria-hidden="true"><Icon name={icon} /></span><span>{name}</span></NavLink>)}</nav><div className="sidebar-bottom"><div className="doctor"><span className="avatar">{user?.name.slice(0, 2).toUpperCase()}</span><div><strong>{user?.name}</strong><small>{user?.role === 'staff' ? 'Secretary' : user?.role}</small></div></div><button className="logout" onClick={logout}>⏻ Log out</button><p className="motto">More Time<br /><span>for What Matters</span></p></div></aside><div className="workspace"><header className="topbar"><span className="workspace-label">Your connected practice</span><div className="account"><NavLink to="/notifications" className="icon-button" aria-label="Notifications">
<Icon name="bell" /></NavLink><span className="avatar small">{user?.name.slice(0, 2).toUpperCase()}</span><strong>{user?.name}</strong><button className="mobile-logout" onClick={logout}>Log out</button></div></header><main><SessionWarning /><Outlet /><footer><span className="footer-dot" />All your locations. One clear picture.<span>Asia/Beirut</span></footer></main></div></>;
}
