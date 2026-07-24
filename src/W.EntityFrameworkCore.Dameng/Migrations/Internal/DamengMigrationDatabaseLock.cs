using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Migrations.Internal;

internal sealed class DamengMigrationDatabaseLock(
    IRelationalCommand releaseLockCommand,
    RelationalCommandParameterObject relationalCommandParameters,
    IHistoryRepository historyRepository)
    : IMigrationsDatabaseLock
{
    public IHistoryRepository HistoryRepository
        => historyRepository;

    public void Dispose()
        => releaseLockCommand.ExecuteScalar(relationalCommandParameters);

    public async ValueTask DisposeAsync()
        => await releaseLockCommand
            .ExecuteScalarAsync(
                relationalCommandParameters,
                CancellationToken.None)
            .ConfigureAwait(false);
}
