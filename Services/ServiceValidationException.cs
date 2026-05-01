namespace XerSize.Services;

public sealed class ServiceValidationException : Exception
{
    public ServiceValidationException(string message)
        : base(message)
    {
    }
}