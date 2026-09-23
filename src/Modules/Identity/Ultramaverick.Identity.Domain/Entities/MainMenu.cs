namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class MainMenu
    {
        public int Id { get; private set; }
        public int? LegacyId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Path { get; private set; } = null!;
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedByUserId { get; private set; }

        private MainMenu() { }

        public static MainMenu Create(string name, string path, int sortOrder)
        {
            name = (name ?? string.Empty).Trim();
            path = (path ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return new MainMenu
            {
                Name = name,
                Path = path,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void Update(string name, string path, int sortOrder, int modifiedByUserId)
        {
            name = (name ?? string.Empty).Trim();
            path = (path ?? string.Empty).Trim();
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            Name = name;
            Path = path;
            SortOrder = sortOrder;
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
