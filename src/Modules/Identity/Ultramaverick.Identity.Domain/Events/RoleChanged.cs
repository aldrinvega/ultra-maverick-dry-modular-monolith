using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Domain.Common;

namespace Ultramaverick.Identity.Domain.Events
{
    public static class RoleChangedTypes
    {
        public const string Created = nameof(Created);
        public const string Updated = nameof(Updated);
        public const string Deactivated = nameof(Deactivated);
        public const string Activated = nameof(Activated);

    }

    public sealed class RoleChangedEvent : IDomainEvent
    {
        public string ChangeType { get; }
        public DateTime OccurredAtUtc { get; }
        public RoleChangedEvent(string changeType)
        {
            ChangeType = changeType;
            OccurredAtUtc = DateTime.UtcNow;
        }
    }
}
