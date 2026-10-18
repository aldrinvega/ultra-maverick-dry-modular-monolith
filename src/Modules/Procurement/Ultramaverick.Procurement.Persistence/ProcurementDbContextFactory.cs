using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ultramaverick.Procurement.Persistence
{
    public sealed class ProcurementDbContextFactory : IDesignTimeDbContextFactory<ProcurementDbContext>
    {
        public ProcurementDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ProcurementConnection")
                ?? "Server=.\\SQLEXPRESS;Database=ElixirDepotDry;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<ProcurementDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ProcurementDbContext(options);
        }
    }
}
