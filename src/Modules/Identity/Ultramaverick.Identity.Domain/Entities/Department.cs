namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class Department
    {
        public int Id { get; private set; }
        public int? LegacyId { get; private set; }
        public string Name { get; private set; } = null!;
        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedByUserId { get; private set; }

        private Department() { }

        public static Department Create(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new Department
            {
                Name = name,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}
