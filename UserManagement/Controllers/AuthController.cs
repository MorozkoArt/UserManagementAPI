using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Services;
using UserManagement.Exceptions;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserManager userManager, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _userManager.AuthenticateAsync(dto.Login, dto.Password);
            return Ok(new { Token = token });
        }
        catch (AuthenticationFailedException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return BadRequest("Authentication error");
        }
    }
}
