namespace be.Domain;

public class CustomException
{
    public class UnauthorizedException(string message = "Unauthorized") : Exception(message)
    {
    }

    public class NotFoundException(string message = "Not found") : Exception(message)
    {
    }

    public class BusinessValidationException(string message = "Validation failed") : Exception(message)
    {
        public object? ValidationData { get; protected set; }
    }

    public class BusinessValidationException<T> : BusinessValidationException
    {
        public BusinessValidationException(T data, string message = "Validation failed") : base(message)
        {
            ValidationData = data;
        }
    }


    public class ConflictException(string message = "Conflict") : Exception(message)
    {
    }

    public class ForbiddenException(string message = "Forbidden") : Exception(message)
    {
    }

    public class TooManyRequestsException(string message = "Too many requests") : Exception(message)
    {
    }
}