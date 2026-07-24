using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

internal sealed class DamengNewGuidTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource)
    : IMethodCallTranslator
{
    private static readonly MethodInfo NewGuid
        = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!;

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => method == NewGuid
            ? sqlExpressionFactory.Function(
                "NEWID",
                arguments: [],
                nullable: false,
                argumentsPropagateNullability: [],
                typeof(Guid),
                typeMappingSource.FindMapping(typeof(Guid)))
            : null;
}
