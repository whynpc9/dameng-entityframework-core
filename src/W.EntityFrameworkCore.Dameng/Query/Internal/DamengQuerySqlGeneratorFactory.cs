using Microsoft.EntityFrameworkCore.Query;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

/// <summary>
/// Creates Dameng query SQL generators.
/// </summary>
internal sealed class DamengQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;

    /// <summary>
    /// Initializes a new query SQL generator factory.
    /// </summary>
    public DamengQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        => _dependencies = dependencies;

    /// <inheritdoc />
    public QuerySqlGenerator Create()
        => new DamengQuerySqlGenerator(_dependencies);
}
