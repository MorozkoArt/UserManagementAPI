namespace UserManagement.Exceptions;

public class CacheNullReturnException : InvalidOperationException
{
    public CacheNullReturnException()
        : base("Cache returned null unexpectedly") { }

    public CacheNullReturnException(string cacheKey)
        : base($"Cache returned null unexpectedly for key: {cacheKey}") { }

    public CacheNullReturnException(string message, Exception innerException)
        : base(message, innerException) { }
}