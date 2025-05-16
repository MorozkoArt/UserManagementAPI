namespace UserManagement.Exceptions;

public class AuthenticationRequiredException : UnauthorizedAccessException
{
    public AuthenticationRequiredException()
        : base("Authentication required") { }

    public AuthenticationRequiredException(string message)
        : base(message) { }

    public AuthenticationRequiredException(string message, Exception innerException)
        : base(message, innerException) { }
}