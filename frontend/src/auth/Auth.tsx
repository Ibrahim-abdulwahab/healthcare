import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { api, post, session } from '../api/client';
import type { Role } from '../lib/permissions';

export interface Identity { userId: number; name: string; email: string; role: Role }
interface AuthState { user: Identity | null; loading: boolean; error: string; login: (email: string, password: string) => Promise<void>; logout: () => void; retry: () => void }
const Context = createContext<AuthState | null>(null);
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Identity | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const logout = () => { session.clear(); setUser(null); setError(''); };
  useEffect(() => {
    let active = true;
    setLoading(true); setError('');
    if (!session.token()) { setLoading(false); return; }
    api<Identity>('/auth/me').then(value => { if (active) setUser({ ...value, userId: Number(value.userId) }); })
      .catch(e => { if (active && session.token()) setError(e.message); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [attempt]);
  useEffect(() => { window.addEventListener('auth-expired', logout); return () => window.removeEventListener('auth-expired', logout); }, []);
  async function login(email: string, password: string) {
    const result = await post<Identity & { token: string }>('/auth/login', { email, password });
    session.set(result.token); setUser(result); setError('');
  }
  return <Context.Provider value={{ user, loading, error, login, logout, retry: () => setAttempt(a => a + 1) }}>{children}</Context.Provider>;
}
export function useAuth() { const value = useContext(Context); if (!value) throw new Error('Authentication context is missing'); return value; }
