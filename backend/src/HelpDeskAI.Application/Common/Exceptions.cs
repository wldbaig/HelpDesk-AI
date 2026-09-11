namespace HelpDeskAI.Application.Common;

public class AppException(string message) : Exception(message);
public sealed class NotFoundException(string message) : AppException(message);
public sealed class ConflictException(string message) : AppException(message);
public sealed class UnauthorizedException(string message) : AppException(message);
public sealed class ExternalServiceException(string message, Exception? inner = null) : AppException(message)
{
    public Exception? RootCause { get; } = inner;
}
