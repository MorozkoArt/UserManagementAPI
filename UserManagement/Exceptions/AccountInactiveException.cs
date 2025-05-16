namespace UserManagement.Exceptions;

public class AccountInactiveException : UnauthorizedAccessException
{
    public AccountInactiveException()
        : base("Your account is inactive") { }

    public AccountInactiveException(string message)
        : base(message) { }

    public AccountInactiveException(string message, Exception innerException)
        : base(message, innerException) { }
}