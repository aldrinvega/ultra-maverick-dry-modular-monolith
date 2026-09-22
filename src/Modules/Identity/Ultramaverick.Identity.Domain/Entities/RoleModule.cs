namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class RoleModule
    {
        public int RoleId { get; private set; }
        public int ModuleId { get; private set; }
        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedByUserId { get; private set; }

        private RoleModule() { }

        public static RoleModule Create(int roleId, int moduleId) => new()
        {
            RoleId = roleId,
            ModuleId = moduleId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        public void Deactivate(int modifiedByUserId)
        {
            if (!IsActive) return;
            IsActive = false;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;
        }

        public void Activate(int modifiedByUserId)
        {
            if (IsActive) return;
            IsActive = true;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;
        }
    }
}
