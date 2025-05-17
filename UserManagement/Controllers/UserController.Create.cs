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
            var currentUser = User.Identity?.Name;
            var user = await _userManager.CreateUserAsync(dto, currentUser ?? "_System_");
            return Ok(new { user.Id, user.Login });
        }
        catch (Exception ex) when (ex is ValidationException or LoginAlreadyExistsException)
        {
            return HandleBadRequest(ex);
        }
        catch (AdminAccessException ex)
        {
            return HandleForbid(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(CreateUser));
        }
    }
}