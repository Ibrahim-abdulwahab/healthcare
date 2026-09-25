namespace Healthcare.Api.Models;

public class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }


    // Relationships

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}