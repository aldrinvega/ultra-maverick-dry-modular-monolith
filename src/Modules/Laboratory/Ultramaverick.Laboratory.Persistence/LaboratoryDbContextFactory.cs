using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Laboratory.Persistence
{
    public sealed class LaboratoryDbContextFactory : IDesignTimeDbContextFactory<LaboratoryDbContext>
    {
        public LaboratoryDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("LaboratoryConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<LaboratoryDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new LaboratoryDbContext(options);
        }
    }
}
