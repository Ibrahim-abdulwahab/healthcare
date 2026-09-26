namespace Healthcare.Api.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;
}