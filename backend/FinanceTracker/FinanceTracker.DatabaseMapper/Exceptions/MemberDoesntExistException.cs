using FinanceTracker.DatabaseMapper.Exceptions.Base;

namespace FinanceTracker.DatabaseMapper.Exceptions;

public class MemberDoesntExistException : CustomExceptionBase
{
    public MemberDoesntExistException(string message, string? cause = null, Exception? innerException = null)
        : base(message, cause, innerException) { }
}