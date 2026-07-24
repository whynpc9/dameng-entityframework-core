using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal sealed class DamengCharTypeMapping : CharTypeMapping
{
    public DamengCharTypeMapping(string storeType)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(char),
                    jsonValueReaderWriter: JsonCharReaderWriter.Instance),
                storeType,
                StoreTypePostfix.None,
                System.Data.DbType.String,
                unicode: true,
                size: 1))
    {
    }

    private DamengCharTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DamengCharTypeMapping(parameters);
}
