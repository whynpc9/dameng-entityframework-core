using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengJsonTypeMapping : JsonTypeMapping
{
    private static readonly ValueConverter<JsonElement, string> JsonConverter = new(
        element => ToJson(element),
        json => FromJson(json));

    private static readonly ValueComparer<JsonElement> JsonComparer = new(
        (left, right) => JsonEquals(left, right),
        element => JsonHashCode(element),
        element => Snapshot(element));

    public static DamengJsonTypeMapping Default { get; } = new("JSON");

    public DamengJsonTypeMapping(string storeType)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(JsonElement),
                    JsonConverter,
                    JsonComparer),
                storeType,
                StoreTypePostfix.None,
                System.Data.DbType.String,
                unicode: true))
    {
    }

    private DamengJsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DamengJsonTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''", StringComparison.Ordinal)}'";

    private static string ToJson(JsonElement element)
        => element.ValueKind == JsonValueKind.Undefined ? "null" : element.GetRawText();

    private static JsonElement FromJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static bool JsonEquals(JsonElement left, JsonElement right)
        => ToJson(left).Equals(ToJson(right), StringComparison.Ordinal);

    private static int JsonHashCode(JsonElement element)
        => StringComparer.Ordinal.GetHashCode(ToJson(element));

    private static JsonElement Snapshot(JsonElement element)
        => element.ValueKind == JsonValueKind.Undefined ? default : element.Clone();
}

internal sealed class DamengStructuralJsonTypeMapping : JsonTypeMapping
{
    private static readonly MethodInfo GetStringMethod =
        typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetString), [typeof(int)])!;
    private static readonly MethodInfo CreateUtf8StreamMethod =
        typeof(DamengStructuralJsonTypeMapping).GetRuntimeMethod(
            nameof(CreateUtf8Stream),
            [typeof(string)])!;

    public static DamengStructuralJsonTypeMapping Default { get; } = new("JSON");

    public DamengStructuralJsonTypeMapping(string storeType)
        : base(storeType, typeof(JsonTypePlaceholder), System.Data.DbType.String)
    {
    }

    private DamengStructuralJsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DamengStructuralJsonTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    public override Expression CustomizeDataReaderExpression(Expression expression)
        => Expression.Call(CreateUtf8StreamMethod, expression);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''", StringComparison.Ordinal)}'";

    public static MemoryStream CreateUtf8Stream(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}
