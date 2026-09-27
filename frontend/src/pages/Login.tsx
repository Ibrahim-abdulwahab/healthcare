import { useState, type FormEvent } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/Auth';
import { ErrorMessage } from '../components/Common';

export function Login() {
  const auth = useAuth(); const location = useLocation();
  const [email, setEmail] = useState(''); const [password, setPassword] = useState('');
  const [error, setError] = useState<unknown>(); const [busy, setBusy] = useState(false);
  if (auth.user) return <Navigate to={(location.state as { from?: string } | null)?.from || '/'} replace />;
  async function submit(event: FormEvent) { event.preventDefault(); setBusy(true); setError(undefined); try { await auth.login(email, password); } catch (e) { setError(e); } finally { setBusy(false); } }
  return <main className="login-page"><section className="login-intro"><div className="brand"><span className="brand-mark">∞</span>OmniSync</div><h1>One doctor.<br />All locations.<br />In sync.</h1><p>A clear view of your practice, wherever you work.</p></section><section className="login-card"><p className="eyebrow">WELCOME BACK</p><h1>Sign in to your practice</h1><p>Use your healthcare account to continue.</p><form onSubmit={submit}><label>Email<input type="email" autoComplete="username" required value={email} onChange={e => setEmail(e.target.value)} /></label><label>Password<input type="password" autoComplete="current-password" required value={password} onChange={e => setPassword(e.target.value)} /></label><ErrorMessage error={error} /><button className="primary-button" disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}</button></form><p className="detail-note">Need an account? Contact your practice administrator.</p></section></main>;
}
