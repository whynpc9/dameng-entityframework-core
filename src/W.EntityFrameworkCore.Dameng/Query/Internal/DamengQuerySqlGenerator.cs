using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

/// <summary>
/// Generates Dameng SQL from EF Core relational query expressions.
/// </summary>
internal sealed class DamengQuerySqlGenerator : QuerySqlGenerator
{
    /// <summary>
    /// Initializes a new query SQL generator.
    /// </summary>
    public DamengQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override string GetOperator(SqlBinaryExpression binaryExpression)
        => binaryExpression is { OperatorType: ExpressionType.Add, Type: not null }
            && binaryExpression.Type == typeof(string)
                ? " || "
                : base.GetOperator(binaryExpression);

    /// <inheritdoc />
    protected override void GeneratePseudoFromClause()
    {
        // Dameng accepts SELECT statements without a FROM clause.
    }
}
