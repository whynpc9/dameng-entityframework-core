using Microsoft.EntityFrameworkCore.Query;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

/// <summary>
/// Creates parameter-based SQL processors for Dameng.
/// </summary>
internal sealed class DamengParameterBasedSqlProcessorFactory(
    RelationalParameterBasedSqlProcessorDependencies dependencies)
    : IRelationalParameterBasedSqlProcessorFactory
{
    public RelationalParameterBasedSqlProcessor Create(
        RelationalParameterBasedSqlProcessorParameters parameters)
        => new DamengParameterBasedSqlProcessor(dependencies, parameters);
}
