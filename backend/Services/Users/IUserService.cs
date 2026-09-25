using Healthcare.Api.DTOs.Users;

namespace Healthcare.Api.Services.Users;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();

    Task<UserDto?> GetByIdAsync(int id);

    Task<UserDto> CreateAsync(CreateUserDto dto);

    Task<UserDto?> UpdateAsync(
        int id,
        UpdateUserDto dto
    );

    Task<bool> DeleteAsync(int id);
}