namespace UserManagement.Models.Dtos;

public class UserUpdateDto
{
    public string? Name { get; set; }
    public int? Gender { get; set; }
    public DateTime? Birthday { get; set; }
}