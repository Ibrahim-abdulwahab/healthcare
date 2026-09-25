// Demo records are separate from rendering; replace them with API responses later.
export const locations = [
  { id: 1, name: 'NINI Hospital', city: 'Beirut', color: 'rose', icon: 'hospital' },
  { id: 2, name: 'City Medical Center', city: 'Tripoli', color: 'blue', icon: 'hospital' },
  { id: 3, name: 'Private Clinic', city: 'Minieh', color: 'green', icon: 'clinic' },
];
export const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
export const schedules = [
  { id: 1, location_id: 1, days: [0, 1, 3], start_time: '08:00', end_time: '13:00' },
  { id: 2, location_id: 2, days: [0, 1, 3], start_time: '13:00', end_time: '18:00' },
  { id: 3, location_id: 3, days: [2], start_time: '09:00', end_time: '17:00' },
  { id: 4, location_id: 2, days: [4], start_time: '09:00', end_time: '13:00' },
];
export function dateKey(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}
export function addDays(date, count) {
  const result = new Date(date);
  result.setDate(result.getDate() + count);
  return result;
}
export function monday(date) { return addDays(date, -((date.getDay() + 6) % 7)); }
export function minutes(time) { const [h, m] = time.split(':').map(Number); return h * 60 + m; }
export function overlaps(a, b) { return a.start_time < b.end_time && b.start_time < a.end_time; }
export function sessionsOn(date, recurring, exceptions) {
  const key = dateKey(date);
  // An exception replaces the entire day's recurring hours.
  const exception = exceptions.find(item => item.date === key);
  if (exception) return exception.is_available ? [exception] : [];
  return recurring.filter(item => item.days.includes((date.getDay() + 6) % 7));
}
const names = [
  ['Maya Khalil', 'Joseph Haddad', 'Rana Saad', 'Pierre Nakhle', 'Tony Gerges', 'Lama Khoury', 'Nadine Chaar'],
  ['Karim Saab', 'Huda Sleiman', 'Ali Mansour', 'Rita Abi Saab', 'Charbel Aoun', 'Fadi Salameh', 'Mariam Daccache'],
  ['Elie Daher', 'Rita Khoury', 'Nabil Azar', 'Samar Hoteit', 'Lina Mansour', 'Walid Harb'],
  ['Paul Maalouf', 'Nour Chamoun', 'Rami Nassar', 'Linda Moussallem', 'George Eid', 'Carla Haddad', 'Michel Kassis'],
  ['Julien Sarkis', 'Dalia Younes', 'Samir Trad', 'Tala Rifi', 'Ziad Moubarak', 'Lara Nasr'],
];
export const appointments = [];
for (let day = 0; day < 5; day++) {
  for (const session of schedules.filter(item => item.days.includes(day))) {
    const count = (minutes(session.end_time) - minutes(session.start_time)) / 30;
    for (let slot = 0; slot < count; slot++) {
      if (day === 2 && slot >= 8 && slot < 10) continue;
      const time = minutes(session.start_time) + slot * 30;
      appointments.push({
        id: `${day}-${session.id}-${slot}`, date: dateKey(addDays(new Date(2026, 8, 14), day)),
        location_id: session.location_id,
        start_time: `${String(Math.floor(time / 60)).padStart(2, '0')}:${String(time % 60).padStart(2, '0')}`,
        patient: names[day][slot % names[day].length], follow_up: slot >= count - 3,
      });
    }
  }
}
