namespace UserManagement.Exceptions;

public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException()
    : base("Invalid credentials") { }

    public AuthenticationFailedException(string message)
        : base() { }

    public AuthenticationFailedException(string message, Exception innerException)
        : base(message, innerException) { }
    

}
