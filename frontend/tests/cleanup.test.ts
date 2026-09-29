import test from 'node:test';
import assert from 'node:assert/strict';
import { displayValue, rowLabel } from '../src/lib/presentation.ts';
import { tokenExpiresAt } from '../src/lib/session.ts';
import { canChangeAppointmentStatus } from '../src/lib/permissions.ts';

test('Reference labels replace IDs while timestamps consistently use Beirut time', () => {
  assert.equal(displayValue({ id: 1, doctorLocationId: 42, doctorLocationName: 'Doctor — Clinic' }, 'doctorLocationId'), 'Doctor — Clinic');
  assert.equal(displayValue({ id: 1, userId: 11, userName: 'Secretary (secretary@example.test)' }, 'userId'), 'Secretary (secretary@example.test)');
  assert.equal(displayValue({ id: 1, staffDoctorId: 4, staffDoctorName: 'Four Doctor' }, 'staffDoctorId'), 'Four Doctor');
  assert.match(displayValue({ id: 1, updatedAt: '2030-01-07T07:00:00Z' }, 'updatedAt'), /09:00/);
  assert.match(displayValue({ id: 1, updatedAt: '2030-07-08T06:00:00Z' }, 'updatedAt'), /09:00/);
  assert.match(displayValue({ id: 1, visitId: 101, visitName: 'Patient — Doctor', visitDatetime: '2030-01-07T07:00:00Z' }, 'visitId'), /Patient — Doctor.*09:00/);
  assert.equal(displayValue({ id: 1, dateOfBirth: '2000-01-01' }, 'dateOfBirth'), '1 Jan 2000');
  assert.match(rowLabel({ id: 3, patientName: 'Patient', doctorName: 'Doctor', createdAt: '2030-01-07T07:00:00Z' }), /Patient.*Doctor.*09:00/);
});
test('Completed visits lock appointment lifecycle for every role without changing role permissions', () => {
  for (const role of ['admin', 'doctor', 'staff'] as const)
    assert.equal(canChangeAppointmentStatus(role, { status: 'booked', hasCompletedVisits: true }), false);
  assert.equal(canChangeAppointmentStatus('staff', { status: 'booked', hasCompletedVisits: false }), true);
  assert.equal(canChangeAppointmentStatus('doctor', { status: 'booked', hasCompletedVisits: false }), false);
  assert.equal(canChangeAppointmentStatus('admin', { status: 'completed', hasCompletedVisits: false }), false);
});
test('JWT expiry parsing is defensive and uses the signed token expiry without extending it', () => {
  const payload = Buffer.from(JSON.stringify({ exp: 1904457600 })).toString('base64url');
  assert.equal(tokenExpiresAt(`header.${payload}.signature`), 1904457600000);
  for (const token of [null, '', 'invalid', 'a.%%%.b', 'a.e30.b']) assert.equal(tokenExpiresAt(token), null);
});
