using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.Dtos;

public class UserLoginUpdateDto
{
    [Required(ErrorMessage = "Login required")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Login should be between 4 and 20 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Login can contain only Latin letters, digits and _-.")]
    public required string NewLogin { get; set; }
}