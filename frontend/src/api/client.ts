const base = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');
const tokenKey = 'healthcare.accessToken';
export const session = {
  token: () => sessionStorage.getItem(tokenKey),
  set: (token: string) => sessionStorage.setItem(tokenKey, token),
  clear: () => sessionStorage.removeItem(tokenKey),
};
export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}
export function query(values: Record<string, string | number | undefined | null>): string {
  return new URLSearchParams(Object.entries(values).filter(([, value]) => value !== undefined && value !== null && value !== '').map(([key, value]) => [key, String(value)])).toString();
}
export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body) headers.set('Content-Type', 'application/json');
  const token = session.token();
  if (token) headers.set('Authorization', `Bearer ${token}`);
  let response: Response;
  try { response = await fetch(`${base}${path}`, { ...options, headers }); }
  catch { throw new ApiError(0, 'Cannot reach the server. Check your connection and try again.'); }
  if (response.status === 401 && path !== '/auth/login') {
    session.clear(); window.dispatchEvent(new Event('auth-expired'));
  }
  if (response.status === 204) return undefined as T;
  const body = await response.json().catch(() => null);
  if (!response.ok) {
    const errors = body?.errors && Object.values(body.errors).flat().join(' ');
    const message = errors || body?.detail || body?.message || ({ 401: 'Your session has expired. Please sign in.', 403: 'You do not have permission for this action.', 404: 'This record is unavailable in your scope.', 409: 'This record changed or the time is no longer available.', 429: 'Too many attempts. Please wait a minute.' } as Record<number, string>)[response.status] || body?.title || 'The request could not be completed.';
    throw new ApiError(response.status, message);
  }
  return body as T;
}
export const post = <T>(path: string, value: unknown) => api<T>(path, { method: 'POST', body: JSON.stringify(value) });
export const put = <T>(path: string, value: unknown) => {
  const expected = value && typeof value === 'object' && 'expectedUpdatedAt' in value ? value.expectedUpdatedAt : undefined;
  return api<T>(path, { method: 'PUT', body: JSON.stringify(value), headers: typeof expected === 'string' ? { 'X-Expected-Updated-At': expected } : undefined });
};
export const patch = <T>(path: string, value: unknown) => api<T>(path, { method: 'PATCH', body: JSON.stringify(value) });
export const remove = (path: string) => api<void>(path, { method: 'DELETE' });

// API pages remain bounded. Used for small option sets and a bounded calendar date range.
export async function pages<T>(path: string): Promise<T[]> {
  const result: T[] = [];
  for (let page = 1; ; page++) {
    const rows = await api<T[]>(`${path}${path.includes('?') ? '&' : '?'}page=${page}&pageSize=200`);
    result.push(...rows);
    if (rows.length < 200) return result;
  }
}
