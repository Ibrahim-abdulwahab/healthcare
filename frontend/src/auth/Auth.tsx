import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { api, post, session } from '../api/client';
import type { Role } from '../lib/permissions';
import { tokenExpiresAt } from '../lib/session';

export interface Identity { userId: number; name: string; email: string; role: Role }
interface AuthState { user: Identity | null; loading: boolean; error: string; sessionNotice: string; expiresIn: number | null; login: (email: string, password: string) => Promise<void>; logout: () => void; retry: () => void }
const Context = createContext<AuthState | null>(null);
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Identity | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [sessionNotice, setSessionNotice] = useState('');
  const [expiresIn, setExpiresIn] = useState<number | null>(null);
  const logout = () => { session.clear(); setUser(null); setError(''); setSessionNotice(''); setExpiresIn(null); };
  const expire = () => { session.clear(); setUser(null); setError(''); setExpiresIn(null); setSessionNotice('Your session expired or is no longer valid. Sign in again to continue. Unsaved form entries are not retained. If a save was in progress, check the record before retrying.'); };
  useEffect(() => {
    let active = true;
    setLoading(true); setError('');
    const restoringToken = session.token();
    if (!restoringToken) { setLoading(false); return; }
    api<Identity>('/auth/me').then(value => { if (active && session.token() === restoringToken) setUser({ ...value, userId: Number(value.userId) }); })
      .catch(e => { if (!active) return; if (!session.token()) expire(); else if (session.token() === restoringToken) setError(e.message); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [attempt]);
  useEffect(() => { window.addEventListener('auth-expired', expire); return () => window.removeEventListener('auth-expired', expire); }, []);
  useEffect(() => {
    if (!user) return;
    const update = () => {
      const deadline = tokenExpiresAt(session.token());
      if (deadline === null) { setExpiresIn(null); return; }
      const seconds = Math.ceil((deadline - Date.now()) / 1000);
      if (seconds <= 0) expire(); else setExpiresIn(seconds <= 120 ? Math.ceil(seconds / 60) * 60 : null);
    };
    update();
    const timer = window.setInterval(update, 1000);
    return () => window.clearInterval(timer);
  }, [user]);
  async function login(email: string, password: string) {
    const result = await post<Identity & { token: string }>('/auth/login', { email, password });
    session.set(result.token); setUser(result); setError(''); setSessionNotice('');
  }
  return <Context.Provider value={{ user, loading, error, sessionNotice, expiresIn, login, logout, retry: () => setAttempt(a => a + 1) }}>{children}</Context.Provider>;
}
export function useAuth() { const value = useContext(Context); if (!value) throw new Error('Authentication context is missing'); return value; }
export function SessionWarning() {
  const { expiresIn } = useAuth();
  return expiresIn !== null && expiresIn <= 120 ? <p role="status" className="error-banner">Your session expires in {Math.max(1, Math.ceil(expiresIn / 60))} minute(s). Save your work before signing in again.</p> : null;
}
