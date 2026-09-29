export type Role = 'admin' | 'doctor' | 'staff';
export function canWrite(role: Role, resource: string, creating = false): boolean {
  if (resource === 'audit-logs') return false;
  if (role === 'admin') return true;
  if (role === 'staff') return ['patients', 'appointments', 'visits', 'paymentrecords'].includes(resource);
  if (resource === 'visits') return !creating;
  return ['schedules', 'schedule-exceptions', 'consultations', 'prescriptions', 'prescriptionitems', 'followups', 'labrequests', 'imagingrequests'].includes(resource);
}
export function canDelete(role: Role, resource: string): boolean { return resource !== 'audit-logs' && (role === 'admin' || (role === 'doctor' && ['schedules', 'schedule-exceptions'].includes(resource))); }
export function canReschedule(role: Role, appointment: { hasVisits: boolean; status: string }): boolean { return role !== 'doctor' && !appointment.hasVisits && !['completed', 'cancelled'].includes(appointment.status); }
export const nextStatus: Record<string, string | undefined> = { booked: 'arrived', arrived: 'waiting', waiting: 'in_consultation', in_consultation: 'completed' };
export function canChangeAppointmentStatus(role: Role, appointment: { status: string; hasCompletedVisits: boolean }): boolean {
  return role !== 'doctor' && !appointment.hasCompletedVisits && !['completed', 'cancelled'].includes(appointment.status);
}
