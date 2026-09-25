using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Appointments;

public class CreateAppointmentDto
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public int VisitTypeId { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    [Required]
    [RegularExpression(
        "^(booked|arrived|waiting|in_consultation|completed|cancelled)$",
        ErrorMessage = "Invalid appointment status."
    )]
    public string Status { get; set; } = "booked";

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }
}