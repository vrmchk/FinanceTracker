using FinanceTracker.DatabaseMapper.Exceptions.Base;

namespace FinanceTracker.DatabaseMapper.Exceptions;

public class MapNotSupportedException : CustomExceptionBase
{
    public MapNotSupportedException(string message, string? cause = null, Exception? innerException = null)
        : base(message, cause, innerException) { }
}