using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Persistence.Repositories;

namespace Ultramaverick.Identity.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IMainMenuRepository, MainMenuRepository>();
            services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
            services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

            return services;
        }
    }
}
