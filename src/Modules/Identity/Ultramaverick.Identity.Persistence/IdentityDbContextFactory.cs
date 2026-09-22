using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Persistence
{
    public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        public IdentityDbContext CreateDbContext(string[] args)
        {
            var connectionString =
           Environment.GetEnvironmentVariable("IdentityConnection")
           ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new IdentityDbContext(options);
        }
    }
}
