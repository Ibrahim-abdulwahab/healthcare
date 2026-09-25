import assert from 'node:assert/strict';
import { dateKey, addDays, monday, minutes, overlaps, sessionsOn, schedules, appointments } from './schedule.js';

// Run: node frontend/schedule.test.mjs
assert.equal(dateKey(monday(new Date(2026, 8, 20))), '2026-09-14');
assert.equal(dateKey(addDays(new Date(2026, 11, 31), 1)), '2027-01-01');
assert.equal(minutes('13:30'), 810);
assert.equal(overlaps(schedules[0], schedules[1]), false, 'Adjacent hours are allowed');
assert.equal(overlaps(schedules[0], { start_time: '12:30', end_time: '14:00' }), true);
assert.equal(sessionsOn(new Date(2026, 8, 14), schedules, []).length, 2);
assert.equal(sessionsOn(new Date(2026, 8, 19), schedules, []).length, 0);
assert.deepEqual(sessionsOn(new Date(2026, 8, 14), schedules, [{ date: '2026-09-14', is_available: false }]), []);
const exception = { date: '2026-09-14', is_available: true, location_id: 3, start_time: '10:00', end_time: '16:00' };
assert.deepEqual(sessionsOn(new Date(2026, 8, 14), schedules, [exception]), [exception]);
assert.ok(appointments.length > 0);
assert.equal(new Set(appointments.map(item => item.id)).size, appointments.length);
for (const appointment of appointments) {
  assert.ok(sessionsOn(new Date(`${appointment.date}T12:00:00`), schedules, []).some(session => session.location_id === appointment.location_id && appointment.start_time >= session.start_time && minutes(appointment.start_time) + 30 <= minutes(session.end_time)));
}
console.log('Schedule checks passed.');
