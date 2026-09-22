using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IIdentityUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IDepartmentRepository Departments { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
