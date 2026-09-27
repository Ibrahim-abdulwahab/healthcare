import { useEffect, useRef, useState, type ReactNode } from 'react';

export function ErrorMessage({ error }: { error: unknown }) { return error ? <p className="error-banner" role="alert">{error instanceof Error ? error.message : String(error)}</p> : null; }
export function Loading() { return <p className="loading" role="status">Loading…</p>; }
export function Empty({ children = 'No records found.' }: { children?: ReactNode }) { return <p className="empty-state">{children}</p>; }
export function Modal({ title, children, close }: { title: string; children: ReactNode; close: () => void }) {
  const ref = useRef<HTMLDialogElement>(null);
  useEffect(() => { ref.current?.showModal(); }, []);
  return <dialog ref={ref} onCancel={e => { e.preventDefault(); close(); }}><div className="dialog-heading"><h2>{title}</h2><button className="icon-button" aria-label="Close dialog" onClick={close}>×</button></div>{children}</dialog>;
}
export function useLoad<T>(load: () => Promise<T>, dependencies: unknown[]) {
  const [data, setData] = useState<T>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>();
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    let active = true; setLoading(true); setError(undefined);
    load().then(value => { if (active) setData(value); }).catch(e => { if (active) setError(e); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [...dependencies, revision]);
  return { data, loading, error, reload: () => setRevision(r => r + 1), setData };
}
export function Status({ value }: { value: string }) { return <span className={`status status-${value}`}>{value.replaceAll('_', ' ')}</span>; }
