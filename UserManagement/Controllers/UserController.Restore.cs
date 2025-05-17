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
            var currentUser = User.Identity?.Name;
            var restoredUser = await _userManager.RestoreUserAsync(login, currentUser ?? string.Empty);
            return Ok(new { restoredUser.Name, restoredUser.IsActive });
        }
        catch (UserNotFoundException ex)
        {
            return HandleNotFound(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(RestoreUser));
        }
    }
}