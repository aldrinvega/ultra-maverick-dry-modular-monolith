using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Persistence.Lookups;

public sealed class ItemUomLookup : IItemUomLookup
{
    private readonly WarehouseDbContext _context;

    public ItemUomLookup(WarehouseDbContext context) => _context = context;

    public async Task<int?> GetUomIdAsync(int itemId, CancellationToken ct)
    {
        await using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [UomId] FROM [Catalog].[Items] WHERE [Id] = @itemId";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@itemId";
        parameter.Value = itemId;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }
}
