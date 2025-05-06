namespace Lib.Domain.Exceptions;

public class RegisterValidationException : Exception
{
    public List<string> Errors { get; set; } = [];
    public RegisterValidationException(string message, List<string>? errors = null) : base(message)
    {
        Errors = errors ?? [];
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
