namespace UserManagement.Models.Dtos;

public class UserResponseDto
{
    public required string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsActive { get; set; }
}