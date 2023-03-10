using FinanceTracker.DatabaseMapper.Exceptions.Base;

namespace FinanceTracker.DatabaseMapper.Exceptions;

public class SameMapAlreadyAddedException : CustomExceptionBase
{
    public SameMapAlreadyAddedException(string message, string? cause = null, Exception? innerException = null)
        : base(message, cause, innerException) { }
}