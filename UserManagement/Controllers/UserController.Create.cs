using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.Controllers;

public partial class UserController
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userManager.CreateUserAsync(dto, currentUser?.Login ?? "_System_");
            return Ok(new { user.Id, user.Login });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (AdminAccessException)
        {
            return Forbid();
        }
        catch (LoginAlreadyExistsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(CreateUser));
        }
    }
}