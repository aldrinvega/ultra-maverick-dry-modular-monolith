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
    }
}
