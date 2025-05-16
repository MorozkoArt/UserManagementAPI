using Microsoft.AspNetCore.Mvc;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.Controllers;
public partial class UserController
{
    [Authorize(Roles = "Admin")]
    [HttpPatch("{login}/restore")]
    public async Task<IActionResult> RestoreUser(string login)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var restoredUser = await _userManager.RestoreUserAsync(login, currentUser?.Login ?? string.Empty);
            return Ok(new { restoredUser.Name, restoredUser.IsActive });
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