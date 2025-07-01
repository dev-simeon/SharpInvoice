namespace SharpInvoice.Modules.Invoicing.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients", "invoicing");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.BusinessId, c.Email });
    }
}