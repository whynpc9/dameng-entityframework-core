using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengDatabaseCreator : RelationalDatabaseCreator
{
    private const string PhysicalDatabaseMessage
        = "Creating or deleting a Dameng database is a server administration operation. "
        + "Configure an existing database and use migrations or EnsureCreated to manage objects in the current schema.";

    public DamengDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies)
        : base(dependencies)
    {
    }

    public override bool Exists()
    {
        var openedHere = Dependencies.Connection.DbConnection.State
            != System.Data.ConnectionState.Open;

        try
        {
            if (openedHere)
            {
                Dependencies.Connection.Open();
            }

            return true;
        }
        finally
        {
            if (openedHere)
            {
                Dependencies.Connection.Close();
            }
        }
    }

    public override async Task<bool> ExistsAsync(
        CancellationToken cancellationToken = default)
    {
        var openedHere = Dependencies.Connection.DbConnection.State
            != System.Data.ConnectionState.Open;

        try
        {
            if (openedHere)
            {
                await Dependencies.Connection
                    .OpenAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            if (openedHere)
            {
                await Dependencies.Connection
                    .CloseAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    public override void Create()
        => throw new NotSupportedException(PhysicalDatabaseMessage);

    public override void Delete()
        => throw new NotSupportedException(PhysicalDatabaseMessage);

    public override bool HasTables()
    {
        var openedHere = Dependencies.Connection.DbConnection.State != System.Data.ConnectionState.Open;

        try
        {
            if (openedHere)
            {
                Dependencies.Connection.Open();
            }

            using var command = Dependencies.Connection.DbConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM USER_TABLES";
            var result = command.ExecuteScalar();

            return Convert.ToInt64(result, CultureInfo.InvariantCulture) > 0;
        }
        finally
        {
            if (openedHere)
            {
                Dependencies.Connection.Close();
            }
        }
    }
}
