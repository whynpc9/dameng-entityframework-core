using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengDateTimeOffsetTypeMapping
    : DateTimeOffsetTypeMapping
{
    private static readonly MethodInfo GetStringMethod =
        typeof(DbDataReader).GetRuntimeMethod(
            nameof(DbDataReader.GetString),
            [typeof(int)])!;

    private static readonly MethodInfo ParseMethod =
        typeof(DamengDateTimeOffsetTypeMapping).GetRuntimeMethod(
            nameof(Parse),
            [typeof(string)])!;

    public DamengDateTimeOffsetTypeMapping(string storeType)
        : base(storeType, System.Data.DbType.DateTimeOffset)
    {
    }

    private DamengDateTimeOffsetTypeMapping(
        RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(
        RelationalTypeMappingParameters parameters)
        => new DamengDateTimeOffsetTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    public override Expression CustomizeDataReaderExpression(
        Expression expression)
        => Expression.Call(ParseMethod, expression);

    public static DateTimeOffset Parse(string value)
        => DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);
}
