using Healthcare.Api.DTOs.Auth;

namespace Healthcare.Api.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
}