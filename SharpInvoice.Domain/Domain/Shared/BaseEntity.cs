namespace SharpInvoice.Core.Domain.Shared;

using System;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
}