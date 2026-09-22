using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Domain.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredAtUtc { get; }
    }

}
