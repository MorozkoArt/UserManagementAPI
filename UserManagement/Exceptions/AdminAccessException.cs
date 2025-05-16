namespace UserManagement.Exceptions;

public class AdminAccessException : UnauthorizedAccessException
{
    public AdminAccessException()
        : base("Only an admin can fulfill this request") { }

    public AdminAccessException(string message)
        : base(message) { }

    public AdminAccessException(string message, Exception innerException)
        : base(message, innerException) { }
}