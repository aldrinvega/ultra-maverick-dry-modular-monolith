using Ultramaverick.Identity.Domain.Common;
using Ultramaverick.Identity.Domain.Events;

namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class Role : Entity
    {
        public int Id { get; private set; }
        public int? LegacyId { get; private set; }
        public string Name { get; private set; } = null!;
        public bool IsActive { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public int? ModifiedByUserId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }

        private Role() { }

        public static Role Create(string name, int? createdByUserId)
        {
            name = (name ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var role = new Role
            {
                Name = name,
                CreatedByUserId = createdByUserId,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            role.RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Created));
            return role;
        }

        public void Rename(string newName, int modifiedByUserId)
        {
            newName = (newName ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(newName);

            Name = newName;
            ModifiedByUserId = modifiedByUserId;
            ModifiedAtUtc = DateTime.UtcNow;

            RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Updated));
        }

        public void Deactivate(int modifiedByUserId)
        {
            if (!IsActive) return;

            IsActive = false;
            ModifiedByUserId = modifiedByUserId;
            ModifiedAtUtc = DateTime.UtcNow;

            RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Deactivated));
        }

        public void Activate(int modifiedByUserId)
        {
            if (IsActive) return;

            IsActive = true;
            ModifiedByUserId = modifiedByUserId;
            ModifiedAtUtc = DateTime.UtcNow;

            RaiseDomainEvent(new RoleChangedEvent(RoleChangedTypes.Activated));
        }
    }
}
