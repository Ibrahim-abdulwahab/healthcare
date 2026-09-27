import { test, expect, type Page } from '@playwright/test';

test.describe.configure({ mode: 'serial' });
let appointmentId: number; let patientId: number; let visitId: number;
const tokens: Record<number, string> = {};
async function login(page: Page, id: number) {
  await page.goto('/login');
  await page.getByLabel('Email', { exact: true }).fill(`user${id}@test.local`);
  await page.getByLabel('Password', { exact: true }).fill('Local-test-password!42');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.locator('.account strong')).toHaveText(`Test ${id}`);
  tokens[id] = (await page.evaluate(() => sessionStorage.getItem('healthcare.accessToken')))!;
}
async function headers(page: Page) { const token = await page.evaluate(() => sessionStorage.getItem('healthcare.accessToken')); return { Authorization: `Bearer ${token}` }; }
async function selectOption(page: Page, label: string, id: number) { await page.getByRole('dialog').getByRole('combobox', { name: label, exact: true }).selectOption(String(id)); }

test('A: secretary registers a patient, books a real available slot, and sees it on the calendar', async ({ page }) => {
  await login(page, 11);
  await page.goto('/book');
  await page.getByRole('button', { name: 'New patient', exact: true }).click();
  const dialog = page.getByRole('dialog');
  await dialog.getByLabel('Medical record number').fill('BROWSER-1');
  await dialog.getByLabel('First name').fill('Browser');
  await dialog.getByLabel('Last name').fill('Patient');
  const createPatient = page.waitForResponse(r => r.url().endsWith('/api/patients') && r.request().method() === 'POST');
  await dialog.getByRole('button', { name: 'Save', exact: true }).click();
  const patientResponse = await createPatient; expect(patientResponse.status()).toBe(201); patientId = (await patientResponse.json()).id;
  await expect(dialog).not.toBeVisible();
  await page.getByLabel('Location', { exact: true }).selectOption('2');
  await page.getByLabel('Date', { exact: true }).fill('2030-01-07');
  await page.getByRole('button', { name: '09:00 – 09:30', exact: true }).click();
  const booking = page.waitForResponse(r => r.url().endsWith('/api/appointments') && r.request().method() === 'POST');
  await page.getByRole('button', { name: 'Book appointment', exact: true }).click();
  const response = await booking; expect(response.status()).toBe(201); const appointment = await response.json(); appointmentId = appointment.id;
  expect(appointment.createdBy).toBe(11); expect(appointment.endDatetime).toContain('07:30:00');
  await expect(page.locator('.calendar-event').filter({ hasText: 'Browser Patient' })).toBeVisible();
  await page.getByLabel('Calendar view', { exact: true }).selectOption('day');
  await expect(page.locator('.day-column')).toHaveCount(1);
  await expect(page.locator('.calendar-event').filter({ hasText: 'Browser Patient' }).locator('time')).toHaveText('09:00');
  await page.getByLabel('Calendar view', { exact: true }).selectOption('week');
  await expect(page.locator('.day-column')).toHaveCount(7);
  await page.screenshot({ path: '../test-results/calendar-desktop.png', fullPage: true });
});

test('B: drag reschedules through API and a conflicting drag preserves the accepted position', async ({ page }) => {
  await login(page, 11);
  await page.goto('/calendar?date=2030-01-07&doctorId=4');
  const event = page.locator('.calendar-event').filter({ hasText: 'Browser Patient' });
  const column = page.locator('.day-column[aria-label="Mon 2030-01-07"]');
  await expect(event).toBeVisible();
  const moveResponse = page.waitForResponse(r => r.url().endsWith(`/api/appointments/${appointmentId}/reschedule`) && r.request().method() === 'PUT');
  await event.dragTo(column, { targetPosition: { x: 80, y: 4 * 70 + 3 } }); // 11:00
  expect((await moveResponse).status()).toBe(200);
  await expect(event.locator('time')).toHaveText('11:00');
  const setup = await page.request.post('/api/appointments', { headers: await headers(page), data: { patientId: 1, doctorId: 4, locationId: 3, visitTypeId: 1, startDatetime: '2030-01-07T10:00:00Z' } }); // 12:00 Beirut
  expect(setup.status()).toBe(201);
  const conflict = page.waitForResponse(r => r.url().endsWith(`/api/appointments/${appointmentId}/reschedule`) && r.request().method() === 'PUT');
  await event.dragTo(column, { targetPosition: { x: 80, y: 5 * 70 + 3 } });
  expect((await conflict).status()).toBe(409);
  await expect(page.getByRole('alert')).toContainText('unavailable');
  await expect(event.locator('time')).toHaveText('11:00');
});

test('C and E: secretary creates a visit, records payment, and cancellations free slots', async ({ page }) => {
  await login(page, 11);
  await page.goto('/records/visits');
  await page.getByRole('button', { name: 'Add visit', exact: true }).click();
  await selectOption(page, 'Appointment', appointmentId);
  await expect(page.getByRole('dialog').getByRole('combobox', { name: 'Patient', exact: true })).toHaveValue(String(patientId));
  const created = page.waitForResponse(r => r.url().endsWith('/api/visits') && r.request().method() === 'POST');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  const response = await created; expect(response.status()).toBe(201); visitId = (await response.json()).id;
  await expect(page.getByRole('dialog')).not.toBeVisible();
  await page.goto('/records/paymentrecords');
  await page.getByRole('button', { name: 'Add payment', exact: true }).click();
  await selectOption(page, 'Visit', visitId);
  await page.getByRole('dialog').getByLabel('Amount collected').fill('42.50');
  await page.getByRole('dialog').getByLabel('Paid to', { exact: false }).selectOption('doctor');
  const paid = page.waitForResponse(r => r.url().endsWith('/api/paymentrecords') && r.request().method() === 'POST');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  const payment = await paid; expect(payment.status()).toBe(201); expect((await payment.json()).collectedBy).toBe(11);
  await page.goto('/reports'); await expect(page.getByRole('cell', { name: '67.5', exact: true })).toBeVisible();
  await page.goto('/calendar?date=2030-01-07&doctorId=4');
  await page.locator('.calendar-event').filter({ hasText: 'Browser Patient' }).click();
  await expect(page.getByRole('dialog').getByRole('link', { name: 'Reschedule', exact: true })).toHaveCount(0);
  await page.getByRole('button', { name: 'Cancel appointment', exact: true }).click();
  await page.getByRole('button', { name: 'Confirm cancellation', exact: true }).click();
  await expect(page.locator('.calendar-event').filter({ hasText: 'Browser Patient' })).toHaveClass(/cancelled-event/);
  const slots = await page.request.get('/api/availability?doctorId=4&locationId=2&visitTypeId=1&date=2030-01-07');
  expect((await slots.json()).some((s: { startDatetime: string }) => s.startDatetime === '2030-01-07T09:00:00Z')).toBe(true);
});

test('D: doctor authors consultation, prescription/items, follow-up, lab and imaging in own patient chart', async ({ page }) => {
  await login(page, 4);
  await page.goto(`/patients/${patientId}`);
  await page.getByRole('tab', { name: 'Consultations', exact: true }).click();
  await page.getByRole('button', { name: 'Add consultation', exact: true }).click();
  await selectOption(page, 'Visit', visitId);
  await page.getByRole('dialog').getByLabel('Reason for visit').fill('Browser workflow review');
  await page.getByRole('dialog').getByLabel('Diagnosis').fill('Test diagnosis');
  const created = page.waitForResponse(r => r.url().endsWith('/api/consultations') && r.request().method() === 'POST');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  const response = await created; expect(response.status()).toBe(201); const consultation = await response.json(); expect(consultation.createdBy).toBe(4);
  await page.getByRole('button', { name: 'View', exact: true }).click();
  await page.getByRole('button', { name: 'Add attachment link', exact: true }).click();
  const attachmentDialog = page.getByRole('dialog').last();
  await attachmentDialog.getByLabel('File name').fill('Browser reference');
  await attachmentDialog.getByLabel('Existing HTTPS file URL').fill('https://example.invalid/browser-reference.pdf');
  const attachmentCreated = page.waitForResponse(r => r.url().endsWith('/api/files') && r.request().method() === 'POST');
  await attachmentDialog.getByRole('button', { name: 'Save', exact: true }).click();
  const attachment = await attachmentCreated; expect(attachment.status()).toBe(201); expect((await attachment.json()).uploadedBy).toBe(4);
  await expect(page.getByRole('link', { name: /Browser reference/ })).toBeVisible();
  await page.getByRole('button', { name: 'Close dialog', exact: true }).click();
  await page.getByRole('tab', { name: 'Prescriptions', exact: true }).click();
  await page.getByRole('button', { name: 'Add prescription', exact: true }).click();
  await selectOption(page, 'Consultation', consultation.id);
  const prescriptionResponse = page.waitForResponse(r => r.url().endsWith('/api/prescriptions') && r.request().method() === 'POST');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  const prescriptionResult = await prescriptionResponse; expect(prescriptionResult.status()).toBe(201); const prescription = await prescriptionResult.json();
  await page.goto(`/records/prescriptionitems?prescriptionId=${prescription.id}`);
  await page.getByRole('button', { name: 'Add prescription item', exact: true }).click();
  await selectOption(page, 'Item', 1);
  await page.getByRole('dialog').getByLabel('Dosage', { exact: true }).fill('As directed');
  await page.getByRole('dialog').getByLabel('Quantity', { exact: true }).fill('5');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('dialog')).not.toBeVisible();
  for (const [resource, button, field, value] of [['labrequests', 'Add lab request', 'Test name', 'Test blood panel'], ['imagingrequests', 'Add imaging request', 'Service name', 'Test scan']] as const) {
    await page.goto(`/records/${resource}`); await page.getByRole('button', { name: button, exact: true }).click();
    await selectOption(page, 'Visit', visitId); await page.getByRole('dialog').getByLabel(field).fill(value);
    await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click(); await expect(page.getByRole('dialog')).not.toBeVisible();
    await expect(page.getByRole('cell', { name: value, exact: true })).toBeVisible();
  }
  await page.goto(`/patients/${patientId}`); await page.getByRole('tab', { name: 'Follow-ups', exact: true }).click();
  await page.getByRole('button', { name: 'Add follow-up', exact: true }).click();
  await page.getByRole('dialog').getByLabel('Notes', { exact: true }).fill('Return for review');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click(); await expect(page.getByRole('dialog')).not.toBeVisible();
  await page.goto('/records/visits');
  await page.getByRole('row').filter({ hasText: 'Browser Patient' }).getByRole('button', { name: 'Edit', exact: true }).click();
  await page.getByRole('dialog').getByRole('combobox', { name: 'Status', exact: true }).selectOption('completed');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('dialog')).not.toBeVisible();
  await expect(page.getByRole('row').filter({ hasText: 'Browser Patient' })).toContainText('completed');
  await page.goto('/calendar?date=2030-01-07&doctorId=4');
  await expect(page.locator('.calendar-event[draggable="true"]')).toHaveCount(0);
  await page.goto('/book'); await expect(page.getByRole('alert')).toContainText('permission');
});

test('F: doctor manages recurring hours and both exception types', async ({ page }) => {
  await login(page, 4);
  await page.goto('/records/schedules'); await page.getByRole('button', { name: 'Add working hours', exact: true }).click();
  await selectOption(page, 'Doctor / location', 2);
  await page.getByRole('dialog').getByLabel('Weekday').selectOption('2');
  await page.getByRole('dialog').getByLabel('Starts (Beirut)').fill('09:00'); await page.getByRole('dialog').getByLabel('Ends (Beirut)').fill('13:00');
  await page.getByRole('dialog').getByLabel('Start interval (minutes)').fill('20');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click(); await expect(page.getByRole('dialog')).not.toBeVisible();
  for (const available of [false, true]) {
    await page.goto('/records/schedule-exceptions'); await page.getByRole('button', { name: 'Add exception', exact: true }).click();
    await selectOption(page, 'Doctor / location', 2); await page.getByRole('dialog').getByLabel('Date', { exact: false }).fill('2030-01-08');
    await page.getByRole('dialog').getByLabel('Starts (Beirut)').fill(available ? '15:00' : '10:00');
    await page.getByRole('dialog').getByLabel('Ends (Beirut)').fill(available ? '17:00' : '11:00');
    await page.getByRole('dialog').getByLabel('Available extra hours').setChecked(available);
    await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click(); await expect(page.getByRole('dialog')).not.toBeVisible();
  }
  const response = await page.request.get('/api/availability?doctorId=4&locationId=2&visitTypeId=1&date=2030-01-08'); const slots = await response.json();
  expect(slots.some((s: { startDatetime: string }) => s.startDatetime === '2030-01-08T08:00:00Z')).toBe(false);
  expect(slots.some((s: { startDatetime: string }) => s.startDatetime === '2030-01-08T13:00:00Z')).toBe(true);
});

test('G: admin manages configuration and inspects reports/audit; non-admin routes stay protected', async ({ page }) => {
  await login(page, 10); await page.goto('/admin');
  await page.getByRole('link', { name: /Visit types/ }).click(); await page.getByRole('button', { name: 'Add visit type', exact: true }).click();
  await page.getByRole('dialog').getByLabel('Name', { exact: false }).fill('Browser review');
  await page.getByRole('dialog').getByLabel('Appointment duration (minutes)').fill('45');
  await page.getByRole('dialog').getByRole('button', { name: 'Save', exact: true }).click(); await expect(page.getByRole('dialog')).not.toBeVisible();
  await expect(page.getByRole('cell', { name: 'Browser review', exact: true })).toBeVisible();
  await page.goto('/records/audit-logs'); await expect(page.getByRole('cell', { name: 'Added', exact: true }).first()).toBeVisible();
  await page.goto('/reports'); await expect(page.getByRole('heading', { name: 'Recorded Collections', exact: true })).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 }); await page.goto('/patients'); await expect(page.getByRole('heading', { name: 'Patients', exact: true })).toBeVisible();
  await page.screenshot({ path: '../test-results/patients-mobile.png', fullPage: true });
});

test('Notifications are personal and secretary consultation view is read-only', async ({ page }) => {
  await login(page, 4); await page.goto('/notifications'); await expect(page.getByText('Private notice', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Mark as read', exact: true }).click(); await expect(page.getByRole('button', { name: 'Mark as read', exact: true })).toHaveCount(0);
  await page.getByRole('button', { name: 'Log out', exact: false }).first().click();
  await expect(page.getByRole('heading', { name: 'Sign in to your practice' })).toBeVisible();
  await login(page, 12); await page.goto('/records/consultations'); await expect(page.getByRole('cell', { name: 'Test diagnosis', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Add consultation', exact: true })).toHaveCount(0);
  await page.goto('/admin'); await expect(page.getByRole('alert')).toContainText('permission');
});

test('Real HTTP access checks cover unrelated staff, another doctor, anonymous requests, files and reports', async ({ page }) => {
  await login(page, 13);
  const unrelated = await headers(page);
  expect((await page.request.put(`/api/appointments/${appointmentId}/reschedule`, { headers: unrelated, data: { doctorId: 4, locationId: 2, visitTypeId: 1, startDatetime: '2030-01-07T13:00:00Z' } })).status()).toBe(404);
  await page.getByRole('button', { name: 'Log out', exact: false }).first().click();
  await login(page, 1);
  const doctorOne = await headers(page);
  expect((await page.request.get('/api/files/1', { headers: doctorOne })).status()).toBe(404);
  expect((await page.request.get('/api/consultations/101', { headers: doctorOne })).status()).toBe(404);
  expect((await page.request.post('/api/schedules', { headers: doctorOne, data: { doctorLocationId: 2, dayOfWeek: 1, startTime: '09:00', endTime: '12:00', slotDuration: 20, isActive: true } })).status()).toBe(403);
  const doctorFour = { Authorization: `Bearer ${tokens[4]}` };
  expect((await page.request.get('/api/patients/3', { headers: doctorFour })).status()).toBe(404);
  expect((await page.request.get('/api/patients/4', { headers: doctorFour })).status()).toBe(200);
  expect((await page.request.put('/api/patients/1', { headers: doctorFour, data: { mrn: 'TEST-1', firstName: 'Forbidden', lastName: 'Test' } })).status()).toBe(403);
  expect((await page.request.get('/api/reports/summary?from=2030-01-01T00:00:00Z&to=2030-02-01T00:00:00Z&doctorId=1', { headers: { Authorization: `Bearer ${tokens[12]}` } })).status()).toBe(403);
  expect((await page.request.get('/api/notifications/1', { headers: doctorOne })).status()).toBe(404);
  expect((await page.request.get('/api/patients')).status()).toBe(401);
  expect((await page.request.get('/api/availability?doctorId=4&locationId=2&visitTypeId=1&date=2030-01-08')).status()).toBe(200);
});

test('Browser displays real validation/scope failures and clears invalid sessions', async ({ page }) => {
  await page.goto('/login');
  await page.evaluate(token => sessionStorage.setItem('healthcare.accessToken', token), tokens[11]);
  await page.goto('/patients');
  await page.getByRole('button', { name: 'Add patient', exact: true }).click();
  const dialog = page.getByRole('dialog');
  await dialog.getByLabel('Medical record number').fill('TEST-1');
  await dialog.getByLabel('First name').fill('Duplicate'); await dialog.getByLabel('Last name').fill('Patient');
  await dialog.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(dialog.getByRole('alert')).toContainText('already exists');
  await page.goto('/patients/2'); await expect(page.getByRole('alert')).toContainText('not found');
  await page.goto('/admin'); await expect(page.getByRole('alert')).toContainText('permission');
  await page.evaluate(() => sessionStorage.setItem('healthcare.accessToken', 'invalid-token'));
  await page.goto('/patients');
  await expect(page.getByRole('heading', { name: 'Sign in to your practice' })).toBeVisible();
  expect(await page.evaluate(() => sessionStorage.getItem('healthcare.accessToken'))).toBeNull();
});

