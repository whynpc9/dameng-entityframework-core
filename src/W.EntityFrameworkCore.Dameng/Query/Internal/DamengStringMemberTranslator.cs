using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

internal sealed class DamengStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    : IMemberTranslator
{
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => member.DeclaringType == typeof(string)
            && member.Name == nameof(string.Length)
                ? sqlExpressionFactory.Function(
                    "LENGTH",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: [true],
                    returnType)
                : null;
}
