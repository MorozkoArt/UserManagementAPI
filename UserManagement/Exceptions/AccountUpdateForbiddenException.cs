namespace UserManagement.Exceptions;

public class AccountUpdateForbiddenException : UnauthorizedAccessException
{
    public AccountUpdateForbiddenException()
        : base("You can only update your own active account") { }

    public AccountUpdateForbiddenException(string message)
        : base(message) { }
    public AccountUpdateForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
