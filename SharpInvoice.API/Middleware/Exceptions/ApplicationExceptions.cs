namespace SharpInvoice.API.Middleware.Exceptions;

/// <summary>
/// Exception thrown when a validation error occurs.
/// </summary>
public class ValidationException(string message) : ApplicationException(message)
{
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"Entity '{name}' with key '{key}' was not found.") { }
}

/// <summary>
/// Exception thrown when a user is not authorized to perform an action.
/// </summary>
public class UnauthorizedException(string message) : ApplicationException(message)
{
}

/// <summary>
/// Exception thrown when a user is forbidden from accessing a resource.
/// </summary>
public class ForbiddenException(string message) : ApplicationException(message)
{
}

/// <summary>
/// Exception thrown when a business logic error occurs.
/// </summary>
public class BusinessLogicException(string message) : ApplicationException(message)
{
} 