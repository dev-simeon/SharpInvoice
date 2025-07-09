using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.Core.Domain.Entities
{
    public sealed class Role : AuditableEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; private set; }

        [MaxLength(500)]
        public string? Description { get; private set; }

        private readonly List<RolePermission> _permissions = new();
        public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

        public ICollection<TeamMember> TeamMembers { get; private set; } = [];

        private Role(Guid id, string name, string? description) : base(id)
        {
            Name = name; Description = description;
        }
        public static Role Create(string name, string? description) => new(Guid.NewGuid(), name, description);
        public void UpdateDescription(string? description) => Description = description;
        private Role() { Name = string.Empty; } // EF Core
    }
}