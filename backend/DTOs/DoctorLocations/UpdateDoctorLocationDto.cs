namespace Healthcare.Api.DTOs.DoctorLocations;

public class UpdateDoctorLocationDto
{
    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public bool IsActive { get; set; }
}