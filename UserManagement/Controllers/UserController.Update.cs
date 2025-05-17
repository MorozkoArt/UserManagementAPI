using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.Controllers;
public partial class UserController
{
    [Authorize]
    [HttpPut("{login}")]
    public async Task<IActionResult> UpdateUser(string login, UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = User.Identity?.Name;
            var updatedUser = await _userManager.UpdateUserAsync(login, dto, currentUser ?? string.Empty);
            return Ok(new { updatedUser.Name, updatedUser.Gender, updatedUser.Birthday });
        }
        catch (Exception ex) when (ex is AuthenticationRequiredException or AccountUpdateForbiddenException)
        {
            return HandleUnauthorized(ex);
        }
        catch (UserNotFoundException ex)
        {
            return HandleNotFound(ex);
        }
        catch (ValidationException ex)
        {
            return HandleBadRequest(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdatePassword));
        }
    }

    [Authorize]
    [HttpPut("{login}/password")]
    public async Task<IActionResult> UpdatePassword(string login, UserPasswordUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = User.Identity?.Name;
            await _userManager.UpdatePasswordAsync(login, dto.NewPassword, currentUser ?? string.Empty);
            return Ok(new { Message = "Password updated successfully" });
        }
        catch (Exception ex) when (ex is AuthenticationRequiredException or AccountUpdateForbiddenException)
        {
            return HandleUnauthorized(ex);
        }
        catch (UserNotFoundException ex)
        {
            return HandleNotFound(ex);
        }
        catch (ValidationException ex)
        {
            return HandleBadRequest(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdatePassword));
        }
    }

    [Authorize]
    [HttpPut("{login}/login")]
    public async Task<IActionResult> UpdateLogin(string login, UserLoginUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = User.Identity?.Name;
            var updatedUser = await _userManager.UpdateLoginAsync(login, dto.NewLogin, currentUser ?? string.Empty);
            return Ok(new { OldLogin = login, NewLogin = updatedUser.Login });
        }

        catch (Exception ex) when (ex is AuthenticationRequiredException or AccountUpdateForbiddenException)
        {
            return HandleUnauthorized(ex);
        }
        catch (Exception ex) when (ex is ValidationException or LoginAlreadyExistsException)
        {
            return HandleBadRequest(ex);
        }
        catch (UserNotFoundException ex)
        {
            return HandleNotFound(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdateLogin));
        }
    }
}