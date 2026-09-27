namespace Healthcare.Api.DTOs.DoctorLocations;

public class DoctorLocationDto
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = "";
    public string LocationName { get; set; } = "";


    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}