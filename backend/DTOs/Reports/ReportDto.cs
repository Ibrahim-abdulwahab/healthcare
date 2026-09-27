using Healthcare.Api.DTOs.Appointments;

namespace Healthcare.Api.DTOs.Reports;

public record StatusCountDto(string Status, int Count);
public record DoctorActivityDto(int DoctorId, string DoctorName, int Appointments, int Visits, int CompletedConsultations);
public record CollectionTotalDto(string PaymentMethod, string PaidTo, decimal Amount, int Records);
public record ReportDto(List<StatusCountDto> AppointmentActivity, List<DoctorActivityDto> DoctorActivity,
    List<CollectionTotalDto> RecordedCollections, List<AppointmentDto> UpcomingAppointments);
