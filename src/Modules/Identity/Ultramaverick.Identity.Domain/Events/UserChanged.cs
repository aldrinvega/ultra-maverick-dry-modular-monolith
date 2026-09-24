using Ultramaverick.SharedKernel;

namespace Ultramaverick.Identity.Domain.Events
{
    public static class UserChangeTypes
    {
        public const string Created = nameof(Created);
        public const string Updated = nameof(Updated);
        public const string Deactivated = nameof(Deactivated);
        public const string Activated = nameof(Activated);
        public const string PasswordChanged = nameof(PasswordChanged);
    }

    public sealed class UserChanged : IDomainEvent
    {
        public string ChangeType { get; }
        public DateTime OccurredAtUtc { get; }

        public UserChanged(string ChangeType, DateTime OccurredAtUtc)
        {
            this.ChangeType = ChangeType;
            this.OccurredAtUtc = OccurredAtUtc;
        }
    }
}
