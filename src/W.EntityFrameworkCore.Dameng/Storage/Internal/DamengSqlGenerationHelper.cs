using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengSqlGenerationHelper : RelationalSqlGenerationHelper
{
    public DamengSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    // Dameng starts a transaction implicitly on the first DML statement. Its ADO.NET
    // command parser rejects both START TRANSACTION and BEGIN TRANSACTION.
    public override string StartTransactionStatement
        => string.Empty;

    public override string GenerateParameterName(string name)
        => name.StartsWith(':') ? name : ":" + name.TrimStart('@');

    public override void GenerateParameterName(StringBuilder builder, string name)
        => builder.Append(':').Append(name.TrimStart('@', ':'));
}
