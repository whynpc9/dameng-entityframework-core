using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengDecimalTypeMapping : DecimalTypeMapping
{
    public DamengDecimalTypeMapping(
        string storeType,
        int? precision = null,
        int? scale = null)
        : base(storeType, System.Data.DbType.Decimal, precision, scale)
    {
    }

    private DamengDecimalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DamengDecimalTypeMapping(parameters);

    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (Precision is >= 0 and <= byte.MaxValue)
        {
            parameter.Precision = (byte)Precision.Value;
        }

        if (Scale is >= 0 and <= byte.MaxValue)
        {
            parameter.Scale = (byte)Scale.Value;
        }
    }
}
