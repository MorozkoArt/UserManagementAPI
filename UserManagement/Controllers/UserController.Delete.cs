namespace UserManagement.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserManagement.Exceptions;

public partial class UserController
{
    [HttpDelete("{login}")]
    public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool softDelete = true)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            await _userManager.DeleteUserAsync(login, currentUser?.Login ?? string.Empty, softDelete);
            return Ok(new { Message = softDelete ? "User soft deleted" : "User permanently deleted" });
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
            return HandleError(ex, nameof(DeleteUser));
        }
    }
}