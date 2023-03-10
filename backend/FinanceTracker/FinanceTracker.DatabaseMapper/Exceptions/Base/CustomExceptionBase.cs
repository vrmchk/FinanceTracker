namespace FinanceTracker.DatabaseMapper.Exceptions.Base;

public abstract class CustomExceptionBase : ApplicationException
{
    protected CustomExceptionBase(string message, string? cause = null, Exception? innerException = null)
        : base(message, innerException)
    {
        CauseOfError = cause;
    }

    public string? CauseOfError { get; }
}