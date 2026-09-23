namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class Module
    {
        public int Id { get; private set; }
        public int? LegacyId { get; private set; }
        public int MainMenuId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? SubMenuName { get; private set; }
        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedByUserId { get; private set; }

        private Module() { }

        public static Module Create(int mainMenuId, string name, string? subMenuName)
        {
            name = (name ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new Module
            {
                MainMenuId = mainMenuId,
                Name = name,
                SubMenuName = subMenuName?.Trim(),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void Update(int mainMenuId, string name, string? subMenuName, int modifiedByUserId)
        {
            name = (name ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            MainMenuId = mainMenuId;
            Name = name;
            SubMenuName = subMenuName?.Trim();
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;
        }

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
