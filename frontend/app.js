import { locations, days, schedules, appointments, dateKey, addDays, monday, minutes, overlaps, sessionsOn } from './schedule.js';

const $ = selector => document.querySelector(selector);
const escapeHtml = value => String(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const paths = {
  dashboard: '<path d="m3 10 9-7 9 7v11h-6v-8H9v8H3Z"/>',
  calendar: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4m10-4v4M3 10h18m-13 4h3m3 0h2m-8 3h3"/>',
  appointments: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4m10-4v4M3 10h18m-14 6 3 3 6-6"/>',
  patients: '<circle cx="9" cy="7" r="3"/><path d="M3 21v-3a6 6 0 0 1 12 0v3m1-17a3 3 0 0 1 0 6m2 4a5 5 0 0 1 3 5v2"/>',
  hospital: '<path d="M8 21V3h8v18M8 8H3v13h18V8h-5M10 7h4m-2-2v4M6 12H5m0 4h1m12-4h1m-1 4h1M10 21v-5h4v5"/>',
  clinic: '<path d="m2 11 10-8 10 8M5 9v12h14V9m-7 2v6m-3-3h6"/>',
  clock: '<circle cx="12" cy="12" r="9"/><path d="M12 6v6l5 3"/>',
  reports: '<path d="M4 21V10h4v11m2 0V3h4v18m2 0V7h4v14M3 21h18"/>',
  settings: '<path d="m9 3-1 3-3 1-2 4 2 2v4l4 3 3-1 3 1 4-3v-4l2-2-2-4-3-1-1-3Z"/><circle cx="12" cy="12" r="3"/>',
  bell: '<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 10h18c0-3-3-3-3-10M10 21h4"/>',
};
const icon = name => `<svg viewBox="0 0 24 24" aria-hidden="true">${paths[name] || paths.calendar}</svg>`;
const navItems = [['dashboard', 'Dashboard'], ['calendar', 'Calendar'], ['appointments', 'Appointments'], ['patients', 'Patients'], ['hospital', 'Locations'], ['clock', 'Availability'], ['reports', 'Reports'], ['settings', 'Settings']];
$('#navigation').innerHTML = navItems.map(([name, label]) => `<a href="#${label.toLowerCase()}" class="nav-link" title="${label}">${icon(name)}<span>${label}</span></a>`).join('');
document.querySelectorAll('[data-icon]').forEach(element => { element.innerHTML = icon(element.dataset.icon); });
$('#notifications').innerHTML = icon('bell');
$('#location-legend').innerHTML = locations.map(location => `<p><i class="swatch ${location.color}"></i>${location.name} (${location.city})</p>`).join('');

// Keep demo changes in memory. Patient information must not be stored in localStorage.
let selectedDate = new Date(2026, 8, 14);
let recurring = structuredClone(schedules);
let bookings = structuredClone(appointments);
let exceptions = [];
let toastTimer;
const locationFor = id => locations.find(location => location.id === id);
const formatDate = (date, options = { day: 'numeric', month: 'short' }) => date.toLocaleDateString('en-GB', options);
function toast(message) {
  $('#toast').textContent = message;
  $('#toast').hidden = false;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { $('#toast').hidden = true; }, 4000);
}
function openDialog(title, content) {
  $('#dialog-title').textContent = title;
  $('#dialog-content').innerHTML = content;
  $('#editor').showModal();
}
$('.close-dialog').onclick = () => $('#editor').close();

function renderCalendar() {
  const isWeek = $('#view').value === 'week';
  const start = isWeek ? monday(selectedDate) : selectedDate;
  const dates = Array.from({ length: isWeek ? 7 : 1 }, (_, index) => addDays(start, index));
  $('#calendar').classList.toggle('day-view', !isWeek);
  $('#date-label').textContent = isWeek ? `${formatDate(start)} – ${formatDate(addDays(start, 6))} ${start.getFullYear()}` : formatDate(start, { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
  $('#previous').setAttribute('aria-label', `Previous ${isWeek ? 'week' : 'day'}`);
  $('#next').setAttribute('aria-label', `Next ${isWeek ? 'week' : 'day'}`);
  $('#calendar').innerHTML = '<div class="day-heading time-heading"></div>' + dates.map(date => `<div class="day-heading ${dateKey(date) === dateKey(new Date()) ? 'is-today' : ''}"><strong>${days[(date.getDay() + 6) % 7]}</strong><span>${formatDate(date)}</span></div>`).join('')
    + `<div class="time-axis">${Array.from({ length: 13 }, (_, index) => `<span class="time-label" style="top:${index / 12 * 100}%">${String(index + 7).padStart(2, '0')}:00</span>`).join('')}</div>`
    + dates.map(date => {
      const sessions = sessionsOn(date, recurring, exceptions);
      return `<div class="day-column ${[0, 6].includes(date.getDay()) ? 'weekend' : ''}" aria-label="${formatDate(date, { weekday: 'long', day: 'numeric', month: 'long' })}">${sessions.map(session => {
        const location = locationFor(session.location_id);
        const visits = bookings.filter(item => item.date === dateKey(date) && item.location_id === location.id && item.start_time >= session.start_time && item.start_time < session.end_time).sort((a, b) => a.start_time.localeCompare(b.start_time));
        return `<section class="session ${location.color}" style="top:calc(${(minutes(session.start_time) - 420) / 720 * 100}% + 3px);height:calc(${(minutes(session.end_time) - minutes(session.start_time)) / 720 * 100}% - 6px)"><div class="session-title">${icon(location.icon)}<div><strong>${location.name}</strong><small>${session.start_time} – ${session.end_time}</small></div></div><div class="session-appointments">${visits.length ? visits.map(visit => `<button class="appointment ${visit.follow_up ? 'followup' : ''}" data-appointment="${visit.id}" title="${visit.start_time} · ${escapeHtml(visit.patient)}${visit.follow_up ? ' · Follow-up' : ''}"><time>${visit.start_time}</time><span>${visit.follow_up ? 'Follow-up' : escapeHtml(visit.patient)}</span></button>`).join('') : '<span class="session-empty">No appointments</span>'}</div></section>`;
      }).join('')}${sessions.length ? '' : `<div class="empty-day">${icon('calendar')}<p>No working hours<br>scheduled</p></div>`}</div>`;
    }).join('');
  $('#working-hours').innerHTML = recurring.map(session => {
    const location = locationFor(session.location_id);
    return `<article class="hours-card ${location.color}"><span class="location-dot"></span>${icon(location.icon)}<div><strong>${location.name}</strong><p>${session.days.map(day => days[day]).join(', ')}<br>${session.start_time} – ${session.end_time}</p></div><button class="icon-button edit-hours" data-schedule="${session.id}" aria-label="Edit ${location.name} working hours">⋯</button></article>`;
  }).join('');
}

function editHours(id) {
  const existing = recurring.find(item => item.id === id);
  const session = existing || { location_id: 1, days: [], start_time: '08:00', end_time: '13:00' };
  openDialog(existing ? 'Edit working hours' : 'Add working hours', `<form id="hours-form">${locationInput(session.location_id)}<fieldset><legend>Repeat every</legend><div class="day-options">${days.map((day, index) => `<label><input type="checkbox" name="days" value="${index}" ${session.days.includes(index) ? 'checked' : ''}>${day}</label>`).join('')}</div></fieldset>${timeInputs(session)}<p class="detail-note">Demo changes last until you refresh this page.</p><p class="form-error" role="alert"></p><button class="primary-button">Save working hours</button>${existing ? '<button type="button" class="danger-button" id="delete-hours">Remove working hours</button>' : ''}</form>`);
  $('#hours-form').onsubmit = event => {
    event.preventDefault();
    const form = new FormData(event.target);
    const updated = { id: existing?.id || Date.now(), location_id: Number(form.get('location_id')), days: form.getAll('days').map(Number), start_time: form.get('start_time'), end_time: form.get('end_time') };
    if (!updated.days.length) return formError('Choose at least one day.');
    if (!validTimes(updated)) return;
    if (recurring.some(other => other.id !== updated.id && other.days.some(day => updated.days.includes(day)) && overlaps(other, updated))) return formError('These hours overlap another location. Choose a free time.');
    const next = [...recurring.filter(other => other.id !== updated.id), updated];
    if (!bookingsCovered(next, exceptions)) return formError('These changes would leave existing appointments outside working hours. Cancel those appointments first.');
    recurring = next;
    saved('Working hours updated');
  };
  if (existing) $('#delete-hours').onclick = () => {
    const next = recurring.filter(item => item.id !== id);
    if (!bookingsCovered(next, exceptions)) return formError('These hours have appointments. Cancel those appointments before removing them.');
    recurring = next;
    saved('Working hours removed');
  };
}
function locationInput(id = 1) { return `<label>Location<select name="location_id">${locations.map(location => `<option value="${location.id}" ${location.id === id ? 'selected' : ''}>${location.name} · ${location.city}</option>`).join('')}</select></label>`; }
function timeInputs(session) { return `<div class="form-row"><label>From<input type="time" name="start_time" min="07:00" max="18:00" step="1800" required value="${session.start_time}"></label><label>Until<input type="time" name="end_time" min="08:00" max="19:00" step="1800" required value="${session.end_time}"></label></div>`; }
function formError(message) { $('.form-error').textContent = message; }
function validTimes(session) {
  if (minutes(session.end_time) - minutes(session.start_time) < 60) { formError('Working hours must be at least one hour long.'); return false; }
  return true;
}
function bookingsCovered(nextRecurring, nextExceptions) {
  return bookings.every(booking => sessionsOn(new Date(`${booking.date}T12:00:00`), nextRecurring, nextExceptions).some(session => session.location_id === booking.location_id && booking.start_time >= session.start_time && minutes(booking.start_time) + 30 <= minutes(session.end_time)));
}
function saved(message) { $('#editor').close(); render(); toast(message); }
function editException() {
  openDialog('Add schedule exception', `<form id="exception-form"><label>Date<input type="date" name="date" required value="${dateKey(selectedDate)}"></label><label>Availability<select name="is_available"><option value="yes">Work at a different location</option><option value="no">Day off · no working hours</option></select></label><div id="exception-hours">${locationInput()}${timeInputs({ start_time: '09:00', end_time: '17:00' })}</div><label>Reason (optional)<input name="reason" maxlength="255" placeholder="For example, covering another clinic"></label><p class="detail-note">Replaces all working hours on this date. Existing appointments must remain within the new hours. Changes last until refresh.</p><p class="form-error" role="alert"></p><button class="primary-button">Save exception</button><button type="button" class="outline-button" id="restore-hours">Restore recurring hours for this date</button></form>`);
  $('#exception-form [name="is_available"]').onchange = event => { $('#exception-hours').hidden = event.target.value === 'no'; $('#exception-hours').querySelectorAll('input,select').forEach(input => { input.disabled = event.target.value === 'no'; }); };
  $('#exception-form').onsubmit = event => {
    event.preventDefault();
    const form = new FormData(event.target);
    const exception = { id: Date.now(), date: form.get('date'), is_available: form.get('is_available') === 'yes', location_id: Number(form.get('location_id')), start_time: form.get('start_time'), end_time: form.get('end_time'), reason: form.get('reason') };
    if (exception.is_available && !validTimes(exception)) return;
    const next = [...exceptions.filter(item => item.date !== exception.date), exception];
    if (!bookingsCovered(recurring, next)) return formError('Appointments on this date conflict with the exception. Cancel them first or choose matching hours.');
    exceptions = next;
    selectedDate = new Date(`${exception.date}T12:00:00`);
    saved('Schedule exception saved');
  };
  $('#restore-hours').onclick = () => {
    const date = $('#exception-form [name="date"]').value;
    exceptions = exceptions.filter(item => item.date !== date);
    saved('Recurring hours restored');
  };
}
function showAppointment(id) {
  const appointment = bookings.find(item => item.id === id);
  if (!appointment) return;
  openDialog(appointment.patient, `<dl class="detail-list"><dt>Location</dt><dd>${locationFor(appointment.location_id).name}</dd><dt>Date</dt><dd>${formatDate(new Date(`${appointment.date}T12:00:00`), { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}</dd><dt>Time</dt><dd>${appointment.start_time} · 30 minutes</dd><dt>Visit type</dt><dd>${appointment.follow_up ? 'Follow-up' : 'Patient appointment'}</dd><dt>Status</dt><dd>Booked</dd></dl><button class="danger-button full" id="cancel-appointment">Cancel appointment</button>`);
  $('#cancel-appointment').onclick = () => {
    $('#dialog-content').innerHTML = `<p class="detail-note">Cancel the appointment for ${escapeHtml(appointment.patient)} at ${appointment.start_time}?</p><div class="form-row"><button class="outline-button" id="keep-appointment">Keep appointment</button><button class="danger-button" id="confirm-cancel">Yes, cancel</button></div>`;
    $('#keep-appointment').onclick = () => showAppointment(id);
    $('#confirm-cancel').onclick = () => { bookings = bookings.filter(item => item.id !== id); saved('Appointment cancelled'); };
  };
}
function table(headers, rows) { return `<div class="table-scroll"><table><thead><tr>${headers.map(header => `<th>${header}</th>`).join('')}</tr></thead><tbody>${rows.map(row => `<tr>${row.map(cell => `<td>${cell}</td>`).join('')}</tr>`).join('')}</tbody></table></div>`; }
function render() {
  const page = location.hash.slice(1) || 'calendar';
  const calendarPage = page === 'calendar' || !navItems.some(([, label]) => label.toLowerCase() === page);
  document.querySelectorAll('.nav-link').forEach(link => { const active = link.hash === `#${calendarPage ? 'calendar' : page}`; link.classList.toggle('active', active); if (active) link.setAttribute('aria-current', 'page'); else link.removeAttribute('aria-current'); });
  $('#calendar-page').hidden = !calendarPage;
  $('#calendar-controls').hidden = !calendarPage;
  $('#secondary-page').hidden = calendarPage;
  $('#page-title').textContent = calendarPage ? `Dr. Rachid – ${$('#view').value === 'week' ? 'Weekly' : 'Daily'} Schedule` : navItems.find(([, label]) => label.toLowerCase() === page)[1];
  $('#page-subtitle').textContent = calendarPage ? 'Multiple locations, one calendar' : 'Your practice at a glance · demo data';
  if (calendarPage) return renderCalendar();
  let content;
  if (page === 'appointments') content = table(['Patient', 'Date', 'Time', 'Location', 'Visit'], bookings.map(item => [escapeHtml(item.patient), item.date, item.start_time, locationFor(item.location_id).name, `<button class="outline-button" data-appointment="${item.id}">${item.follow_up ? 'Follow-up' : 'View details'}</button>`]));
  if (page === 'patients') content = table(['Patient', 'Scheduled visits'], [...new Set(bookings.map(item => item.patient))].sort().map(name => [escapeHtml(name), bookings.filter(item => item.patient === name).length]));
  if (page === 'locations') content = table(['Location', 'City', 'Status'], locations.map(item => [item.name, item.city, 'Active']));
  if (page === 'availability') content = `<h2>Recurring working hours</h2>${table(['Location', 'Days', 'Hours', ''], recurring.map(item => [locationFor(item.location_id).name, item.days.map(day => days[day]).join(', '), `${item.start_time} – ${item.end_time}`, `<button class="outline-button" data-schedule="${item.id}">Edit hours</button>`]))}<p><button class="outline-button" id="availability-add">＋ Add working hours</button></p><h2>Schedule exceptions</h2>${exceptions.length ? table(['Date', 'Availability', 'Reason'], exceptions.map(item => [item.date, item.is_available ? `${locationFor(item.location_id).name} · ${item.start_time}–${item.end_time}` : 'Day off', escapeHtml(item.reason || '—')])) : '<p>No exceptions added.</p>'}`;
  if (page === 'dashboard' || page === 'reports') content = `<h2>${page === 'reports' ? 'Appointment overview' : 'Welcome back, Dr. Rachid'}</h2><p>Sample appointments for 14–20 September 2026.</p><div class="summary-cards"><article><span>Scheduled appointments</span><strong>${bookings.length}</strong></article><article><span>Follow-up visits</span><strong>${bookings.filter(item => item.follow_up).length}</strong></article><article><span>Connected locations</span><strong>${locations.length}</strong></article></div>`;
  if (page === 'settings') content = '<h2>Workspace information</h2><dl class="detail-list"><dt>Doctor</dt><dd>Dr. Rachid</dd><dt>Specialty</dt><dd>Cardiology</dd><dt>Timezone</dt><dd>Asia/Beirut</dd><dt>Data mode</dt><dd>Demo · changes reset on refresh</dd></dl><p>This frontend is not connected to the database. API routes and authentication are needed before using real patient information.</p>';
  $('#secondary-page').innerHTML = content;
  if ($('#availability-add')) $('#availability-add').onclick = () => editHours();
}
document.addEventListener('click', event => {
  const appointment = event.target.closest('[data-appointment]');
  const schedule = event.target.closest('[data-schedule]');
  if (appointment) showAppointment(appointment.dataset.appointment);
  if (schedule) editHours(Number(schedule.dataset.schedule));
});
$('#add-hours').onclick = () => editHours();
$('#add-exception').onclick = editException;
$('#view').onchange = render;
$('#previous').onclick = () => { selectedDate = addDays(selectedDate, $('#view').value === 'week' ? -7 : -1); render(); };
$('#next').onclick = () => { selectedDate = addDays(selectedDate, $('#view').value === 'week' ? 7 : 1); render(); };
$('#today').onclick = () => { selectedDate = new Date(); render(); };
$('#date-button').onclick = () => {
  openDialog('Go to date', `<form id="date-form"><label>Date<input type="date" name="date" required value="${dateKey(selectedDate)}"></label><button class="primary-button">Show schedule</button></form>`);
  $('#date-form').onsubmit = event => { event.preventDefault(); selectedDate = new Date(`${new FormData(event.target).get('date')}T12:00:00`); $('#editor').close(); render(); };
};
$('#notifications').onclick = () => openDialog('Notifications', '<p class="detail-note">You’re all caught up. There are no notifications in this demo workspace.</p>');
window.addEventListener('hashchange', render);
render();
