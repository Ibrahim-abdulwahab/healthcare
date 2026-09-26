using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Healthcare.Api.Configuration;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Auth;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Healthcare.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HealthcareDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        HealthcareDbContext context,
        IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto dto)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !user.IsActive)
            return null;

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        var expiresAt = DateTime.UtcNow.AddMinutes(
            _jwtSettings.ExpirationMinutes
        );

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()
            ),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new(
                ClaimTypes.Name,
                user.Name
            ),

            new(
                ClaimTypes.Email,
                user.Email
            ),

            new(
                ClaimTypes.Role,
                user.Role
            ),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString =
            new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}