namespace SharpInvoice.Core.Domain.Enums;

public enum BusinessRole
{
    /// <summary>
    /// The super user for a business. Can manage billing and delete the business.
    /// </summary>
    Owner = 1,
    /// <summary>
    /// Can manage users, settings, clients, and invoices.
    /// </summary>
    Admin = 2,
    /// <summary>
    /// Can manage clients and invoices but not settings or other users.
    /// </summary>
    Manager = 3,
    /// <summary>
    /// Can create and edit invoices but not send them or manage clients.
    /// </summary>
    Editor = 4,
    /// <summary>
    /// Has read-only access to invoices and financial reports.
    /// </summary>
    Accountant = 5
} 