namespace UserManagement.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public int Gender { get; set; } 
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }

    public bool IsActive => RevokedOn == null;
}