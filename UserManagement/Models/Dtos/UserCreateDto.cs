using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace UserManagement.Models.Dtos;

public class UserCreateDto
{
    [Required(ErrorMessage = "Login required")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Login should be between 4 and 20 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Login can contain only Latin letters, digits and _-.")]
    public required string Login { get; set; }

    [Required(ErrorMessage = "Password required")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "The password must be between 8 and 64 characters")]
    [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+=\[\]{};':"",./<>?\\|`~-]*$", ErrorMessage = "The password can only contain Latin letters, numbers and special characters")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Name required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "The name should be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "The name can only contain letters, spaces, and hyphens")]
    public required string Name { get; set; }

    [RegularExpression(@"^[0-2]+$", ErrorMessage = "The gender value can contain only 3 values (0 - unknown, 1 - man, 2 - women)")]
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
}