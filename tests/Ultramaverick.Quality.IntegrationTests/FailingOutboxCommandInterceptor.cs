using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Forces the outbox flush inside SaveChanges to fail by throwing on the command that
/// targets the OutboxMessages table. The QC-receipt insert runs on the preceding save, so
/// this makes the failure land after a row has already been written.
/// </summary>
internal sealed class FailingOutboxCommandInterceptor : DbCommandInterceptor
{
    private const string OutboxTable = "OutboxMessages";

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ThrowIfOutbox(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ThrowIfOutbox(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        ThrowIfOutbox(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThrowIfOutbox(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void ThrowIfOutbox(DbCommand command)
    {
        if (command.CommandText.Contains(OutboxTable, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Simulated failure while flushing the outbox.");
        }
    }
}
