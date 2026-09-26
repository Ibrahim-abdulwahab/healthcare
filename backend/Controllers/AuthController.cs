using Healthcare.Api.DTOs.Auth;
using Healthcare.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin-test")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            message = "You are authorized as an admin."
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value,

            name = User.Identity?.Name,

            email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email
            )?.Value,

            role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role
            )?.Value
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }
}