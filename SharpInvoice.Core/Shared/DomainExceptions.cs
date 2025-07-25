namespace SharpInvoice.Core.Shared;

/// <summary>
/// Base exception for all domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException(string entityName, object entityId) : DomainException($"Entity '{entityName}' with ID '{entityId}' was not found.")
{
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException(string message) : DomainException(message)
{
}

/// <summary>
/// Exception thrown when an invoice cannot be modified or processed.
/// </summary>
public class InvoiceOperationException(string message) : DomainException(message)
{
}

/// <summary>
/// Exception thrown when a payment operation fails.
/// </summary>
public class PaymentOperationException(string message) : DomainException(message)
{
}

/// <summary>
/// Exception thrown when an authentication or authorization operation fails.
/// </summary>
public class AuthenticationException(string message) : DomainException(message)
{
}

/// <summary>
/// Exception thrown when an action on an inactive business is attempted.
/// </summary>
public class InactiveBusinessException(string businessId) : DomainException($"Cannot perform operation on inactive or deleted business (ID: {businessId}).")
{
}

/// <summary>
/// Exception thrown when a client with invoices is attempted to be deleted.
/// </summary>
public class ClientWithInvoicesException(string clientId) : DomainException($"Cannot delete client with ID '{clientId}' because they have associated invoices.")
{
}

/// <summary>
/// Exception thrown when a validation fails within the domain.
/// </summary>
public class DomainValidationException(string message) : DomainException(message)
{
}

/// <summary>
/// Exception thrown when trying to use an expired token.
/// </summary>
public class TokenExpiredException(string tokenType) : DomainException($"The {tokenType} token has expired.")
{
} 