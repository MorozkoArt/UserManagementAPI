namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(DeleteUser));
        }
    }
}