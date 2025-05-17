using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;
using UserManagement.Services.Interfaces;


namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserManager userManager, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<AuthController> _logger = logger;

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
