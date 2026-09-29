// Display/preflight convenience only. The server still validates every JWT.
export function tokenExpiresAt(token: string | null): number | null {
  try {
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1].replaceAll('-', '+').replaceAll('_', '/')));
    return typeof payload.exp === 'number' && Number.isFinite(payload.exp) ? payload.exp * 1000 : null;
  } catch { return null; }
}
