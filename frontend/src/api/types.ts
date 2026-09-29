// API response contracts generated from the existing backend DTOs.

export interface AppointmentDto {
  patientName: string;
  doctorName: string;
  locationName: string;
  visitTypeName: string;

  id: number;
  patientId: number;
  doctorId: number;
  locationId: number;
  visitTypeId: number;
  startDatetime: string;
  endDatetime: string;
  status: string;
  notes: string | null;
  createdBy: number;
  createdAt: string;
  updatedAt: string;
  hasVisits: boolean;
  hasCompletedVisits: boolean;
}

export interface AuditLogDto {
  userName: string;
  id: number;
  userId: number;
  action: string;
  tableName: string;
  recordId: number;
  createdAt: string;
}

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  token: string;
  expiresAt: string;
  userId: number;
  name: string;
  email: string;
  role: string;
}

export interface AvailableSlotDto {
  startDatetime: string;
  endDatetime: string;
}

export interface CodeListDto {
  id: number;
  category: string;
  code: string;
  valueEn: string;
  valueAr: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface ConsultationDto {
  patientName: string;

  id: number;
  visitId: number;
  reasonForVisit: string | null;
  diagnosis: string | null;
  notes: string | null;
  followUpPlan: string | null;
  nextAppointmentId: number | null;
  createdBy: number;
  createdAt: string;
  updatedAt: string;
}

export interface DoctorLocationDto {
  doctorName: string;
  locationName: string;

  id: number;
  doctorId: number;
  locationId: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface DoctorDto {
  id: number;
  organizationId: number;
  firstName: string;
  lastName: string;
  specialty: string | null;
  phone: string | null;
  email: string | null;
  licenseNumber: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface FollowUpDto {
  id: number;
  patientId: number;
  doctorId: number;
  appointmentId: number | null;
  notes: string | null;
  reminderDate: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface HealthcareFileDto {
  id: number;
  tableName: string;
  recordId: number;
  fileName: string;
  fileUrl: string;
  uploadedBy: number;
  createdAt: string;
}

export interface ImagingRequestDto {
  id: number;
  visitId: number;
  serviceName: string;
  bodyPart: string | null;
  priority: string | null;
  status: string;
  scheduledDate: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ItemCategoryDto {
  id: number;
  name: string;
  description: string | null;
  parentId: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ItemDto {
  id: number;
  code: string;
  name: string;
  genericName: string | null;
  brandName: string | null;
  type: string;
  unit: string;
  categoryId: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface LabRequestDto {
  id: number;
  visitId: number;
  testName: string;
  priority: string | null;
  status: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LocationDto {
  id: number;
  organizationId: number;
  name: string;
  address: string | null;
  city: string | null;
  phone: string | null;
  roomNumber: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface NotificationDto {
  id: number;
  userId: number;
  type: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface OrganizationDto {
  id: number;
  name: string;
  type: string;
  address: string | null;
  phone: string | null;
  email: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PatientDto {
  id: number;
  mrn: string;
  firstName: string;
  lastName: string;
  sex: string | null;
  dateOfBirth: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  nationalId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PaymentRecordDto {
  visitName: string;
  visitDatetime: string;
  collectedByName: string;
  id: number;
  visitId: number;
  amount: number;
  paymentMethod: string;
  collectedBy: number;
  paidTo: string;
  paymentStatus: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PermissionDto {
  id: number;
  name: string;
  code: string;
  description: string | null;
}

export interface PrescriptionItemDto {
  id: number;
  prescriptionId: number;
  itemId: number;
  dosage: string | null;
  frequency: string | null;
  durationDays: number | null;
  quantity: number | null;
  route: string | null;
  instructions: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PrescriptionDto {
  patientName: string;
  doctorName: string;

  id: number;
  consultationId: number;
  patientId: number;
  doctorId: number;
  prescriptionDate: string;
  notes: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface RoleDto {
  id: number;
  name: string;
  description: string | null;
}

export interface ScheduleExceptionDto {
  doctorLocationName: string;
  id: number;
  doctorLocationId: number;
  date: string;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
  reason: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ScheduleDto {
  doctorLocationName: string;
  id: number;
  doctorLocationId: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  slotDuration: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SystemSettingDto {
  id: number;
  key: string;
  value: string | null;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface UserRoleDto {
  id: number;
  userId: number;
  roleId: number;
}

export interface UserDto {
  staffDoctorName: string | null;
  staffOrganizationName: string | null;
  id: number;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  staffDoctorId: number | null;
  staffOrganizationId: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface VisitDto {
  patientName: string;
  doctorName: string;
  locationName: string;

  id: number;
  appointmentId: number;
  patientId: number;
  doctorId: number;
  locationId: number;
  status: string;
  checkInTime: string | null;
  checkOutTime: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface VisitTypeDto {
  id: number;
  name: string;
  description: string | null;
  defaultDuration: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ReportDto {
  appointmentActivity: { status: string; count: number }[];
  doctorActivity: { doctorId: number; doctorName: string; appointments: number; visits: number; completedConsultations: number }[];
  recordedCollections: { paymentMethod: string; paidTo: string; amount: number; records: number }[];
  upcomingAppointments: AppointmentDto[];
}
export type DataRow = { id: number; [key: string]: unknown };
