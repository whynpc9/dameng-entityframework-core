using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

/// <summary>
/// Converts between Dameng BIT value expressions and SQL search conditions.
/// </summary>
/// <remarks>
/// Dameng does not accept a BIT value directly in WHERE, HAVING, or JOIN
/// predicates. Conversely, predicate expressions such as EXISTS cannot be
/// projected directly and must be converted to a BIT value.
/// </remarks>
internal sealed class DamengSearchConditionConverter(
    ISqlExpressionFactory sqlExpressionFactory)
    : ExpressionVisitor
{
    [return: NotNullIfNotNull(nameof(expression))]
    public override Expression? Visit(Expression? expression)
        => Visit(expression, inSearchConditionContext: false);

    [return: NotNullIfNotNull(nameof(expression))]
    private Expression? Visit(
        Expression? expression,
        bool inSearchConditionContext)
        => expression switch
        {
            CaseExpression caseExpression
                => VisitCase(caseExpression, inSearchConditionContext),
            SelectExpression selectExpression
                => VisitSelect(selectExpression),
            SqlBinaryExpression binaryExpression
                => VisitSqlBinary(binaryExpression, inSearchConditionContext),
            SqlUnaryExpression unaryExpression
                => VisitSqlUnary(unaryExpression, inSearchConditionContext),
            PredicateJoinExpressionBase joinExpression
                => VisitPredicateJoin(joinExpression),
            SqlExpression sqlExpression
                and (ExistsExpression
                    or InExpression
                    or LikeExpression)
                => ApplyConversion(
                    (SqlExpression)base.VisitExtension(sqlExpression),
                    inSearchConditionContext,
                    isSearchCondition: true),
            SqlExpression sqlExpression
                => ApplyConversion(
                    (SqlExpression)base.VisitExtension(sqlExpression),
                    inSearchConditionContext,
                    isSearchCondition: false),
            _ => base.Visit(expression)
        };

    private SqlExpression ApplyConversion(
        SqlExpression expression,
        bool inSearchConditionContext,
        bool isSearchCondition)
        => (inSearchConditionContext, isSearchCondition) switch
        {
            (true, false) => sqlExpressionFactory.Equal(
                expression,
                sqlExpressionFactory.Constant(true)),
            (false, true) => sqlExpressionFactory.Case(
                [
                    new CaseWhenClause(
                        expression,
                        sqlExpressionFactory.ApplyDefaultTypeMapping(
                            sqlExpressionFactory.Constant(true)))
                ],
                sqlExpressionFactory.ApplyDefaultTypeMapping(
                    sqlExpressionFactory.Constant(false))),
            _ => expression
        };

    private SqlExpression VisitCase(
        CaseExpression caseExpression,
        bool inSearchConditionContext)
    {
        var testIsSearchCondition = caseExpression.Operand is null;
        var operand = (SqlExpression?)Visit(
            caseExpression.Operand,
            inSearchConditionContext: false);
        var whenClauses = new List<CaseWhenClause>(
            caseExpression.WhenClauses.Count);

        foreach (var whenClause in caseExpression.WhenClauses)
        {
            whenClauses.Add(
                new CaseWhenClause(
                    (SqlExpression)Visit(
                        whenClause.Test,
                        testIsSearchCondition)!,
                    (SqlExpression)Visit(
                        whenClause.Result,
                        inSearchConditionContext: false)!));
        }

        var elseResult = (SqlExpression?)Visit(
            caseExpression.ElseResult,
            inSearchConditionContext: false);

        return ApplyConversion(
            sqlExpressionFactory.Case(
                operand,
                whenClauses,
                elseResult,
                caseExpression),
            inSearchConditionContext,
            isSearchCondition: false);
    }

    private JoinExpressionBase VisitPredicateJoin(PredicateJoinExpressionBase join)
        => join.Update(
            (TableExpressionBase)Visit(join.Table)!,
            (SqlExpression)Visit(
                join.JoinPredicate,
                inSearchConditionContext: true)!);

    private SelectExpression VisitSelect(SelectExpression select)
    {
        var tables = select.Tables
            .Select(table => (TableExpressionBase)Visit(table)!)
            .ToList();
        var predicate = (SqlExpression?)Visit(
            select.Predicate,
            inSearchConditionContext: true);
        var groupBy = select.GroupBy
            .Select(expression => (SqlExpression)Visit(expression)!)
            .ToList();
        var having = (SqlExpression?)Visit(
            select.Having,
            inSearchConditionContext: true);
        var projections = select.Projection
            .Select(projection => (ProjectionExpression)Visit(projection)!)
            .ToList();
        var orderings = select.Orderings
            .Select(ordering => (OrderingExpression)Visit(ordering)!)
            .ToList();
        var offset = (SqlExpression?)Visit(
            select.Offset,
            inSearchConditionContext: false);
        var limit = (SqlExpression?)Visit(
            select.Limit,
            inSearchConditionContext: false);

        return select.Update(
            tables,
            predicate,
            groupBy,
            having,
            projections,
            orderings,
            offset,
            limit);
    }

    private SqlExpression VisitSqlBinary(
        SqlBinaryExpression binary,
        bool inSearchConditionContext)
    {
        var operandsAreSearchConditions = binary.OperatorType
            is ExpressionType.AndAlso
            or ExpressionType.OrElse;
        var left = (SqlExpression)Visit(
            binary.Left,
            operandsAreSearchConditions)!;
        var right = (SqlExpression)Visit(
            binary.Right,
            operandsAreSearchConditions)!;
        var updated = binary.Update(left, right);
        var isSearchCondition = binary.OperatorType
            is ExpressionType.AndAlso
            or ExpressionType.OrElse
            or ExpressionType.Equal
            or ExpressionType.NotEqual
            or ExpressionType.GreaterThan
            or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan
            or ExpressionType.LessThanOrEqual;

        return ApplyConversion(
            updated,
            inSearchConditionContext,
            isSearchCondition);
    }

    private SqlExpression VisitSqlUnary(
        SqlUnaryExpression unary,
        bool inSearchConditionContext)
    {
        var isBooleanNot = unary.OperatorType == ExpressionType.Not
            && unary.Type == typeof(bool);
        var operandIsSearchCondition = isBooleanNot;
        var isSearchCondition = isBooleanNot
            || unary.OperatorType
                is ExpressionType.Equal
                or ExpressionType.NotEqual;
        var operand = (SqlExpression)Visit(
            unary.Operand,
            operandIsSearchCondition)!;

        return ApplyConversion(
            unary.Update(operand),
            inSearchConditionContext,
            isSearchCondition);
    }
}
