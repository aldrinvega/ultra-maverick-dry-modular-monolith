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
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new Module
            {
                MainMenuId = mainMenuId,
                Name = name,
                SubMenuName = subMenuName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}
