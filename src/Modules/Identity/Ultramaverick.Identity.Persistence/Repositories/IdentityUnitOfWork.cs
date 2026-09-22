using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Persistence.Repositories
{
    public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
    {
        private readonly IdentityDbContext _context;

        public IdentityUnitOfWork(IdentityDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Roles = new RoleRepository(context);
            Departments = new DepartmentRepository(context);
        }

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IDepartmentRepository Departments { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
    }
}
