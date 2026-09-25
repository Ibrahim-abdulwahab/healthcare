namespace Healthcare.Api.DTOs.DoctorLocations;

public class CreateDoctorLocationDto
{
    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public bool IsActive { get; set; } = true;
}