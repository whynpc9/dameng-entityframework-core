using System.Data.Common;
using Dm;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengRelationalConnection : RelationalConnection
{
    public DamengRelationalConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override DbConnection CreateDbConnection()
        => new DmConnection(ConnectionString);
}
