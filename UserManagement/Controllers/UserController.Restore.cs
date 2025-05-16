namespace UserManagement.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserManagement.Exceptions;

public partial class UserController
{
    [HttpPatch("{login}/restore")]
    public async Task<IActionResult> RestoreUser(string login)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var restoredUser = await _userManager.RestoreUserAsync(login, currentUser?.Login ?? string.Empty);
            return Ok(new { restoredUser.Name, restoredUser.IsActive });
        }
        catch (AdminAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(RestoreUser));
        }
    }
}