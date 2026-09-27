import test from 'node:test';
import assert from 'node:assert/strict';
import { addDays, dateKey, dayStart, monday, toUtc, clockTime } from '../src/lib/time.ts';
import { canDelete, canReschedule, canWrite } from '../src/lib/permissions.ts';

test('Beirut winter and summer conversion is independent of browser timezone', () => {
  assert.equal(toUtc('2030-01-07T09:00'), '2030-01-07T07:00:00.000Z');
  assert.equal(toUtc('2030-07-08T09:00'), '2030-07-08T06:00:00.000Z');
  assert.equal(clockTime('2030-01-07T07:00:00Z'), '09:00');
  assert.equal(dateKey(new Date('2030-01-07T23:00:00Z')), '2030-01-08');
  assert.equal(dayStart('2030-01-07'), '2030-01-06T22:00:00.000Z');
});
test('Calendar arithmetic crosses month and year boundaries without timezone drift', () => {
  assert.equal(addDays('2029-12-31', 1), '2030-01-01');
  assert.equal(monday('2030-01-13'), '2030-01-07');
});
test('UI permissions preserve clinical authorship and visit-locked appointments', () => {
  assert.equal(canWrite('doctor', 'patients'), false);
  assert.equal(canWrite('doctor', 'appointments'), false);
  assert.equal(canWrite('staff', 'consultations'), false);
  assert.equal(canWrite('doctor', 'consultations'), true);
  assert.equal(canWrite('staff', 'patients', true), true);
  assert.equal(canDelete('doctor', 'prescriptionitems'), false);
  assert.equal(canDelete('admin', 'audit-logs'), false);
  for (const role of ['admin', 'staff'] as const) {
    assert.equal(canReschedule(role, { hasVisits: false, status: 'booked' }), true);
    assert.equal(canReschedule(role, { hasVisits: true, status: 'booked' }), false);
    assert.equal(canReschedule(role, { hasVisits: false, status: 'completed' }), false);
  }
  assert.equal(canReschedule('doctor', { hasVisits: false, status: 'booked' }), false);
});
