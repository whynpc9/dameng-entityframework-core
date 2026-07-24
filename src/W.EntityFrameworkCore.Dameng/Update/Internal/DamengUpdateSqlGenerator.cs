using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace W.EntityFrameworkCore.Dameng.Update.Internal;

internal sealed class DamengUpdateSqlGenerator : UpdateAndSelectSqlGenerator
{
    public DamengUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override void AppendRowsAffectedWhereCondition(
        StringBuilder commandStringBuilder,
        int expectedRowsAffected)
        => commandStringBuilder
            .Append("SQL%ROWCOUNT = ")
            .Append(expectedRowsAffected);

    protected override void AppendIdentityWhereCondition(
        StringBuilder commandStringBuilder,
        IColumnModification columnModification)
    {
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
        commandStringBuilder.Append(" = ");

        var property = columnModification.Property;
        if (property?.GetDamengValueGenerationStrategy()
            == DamengValueGenerationStrategy.Sequence)
        {
            var sequenceName = property.GetDamengSequenceName()
                ?? property.DeclaringType.GetRootType().ShortName() + "Sequence";
            SqlGenerationHelper.DelimitIdentifier(
                commandStringBuilder,
                sequenceName,
                property.GetDamengSequenceSchema());
            commandStringBuilder.Append(".CURRVAL");
        }
        else
        {
            commandStringBuilder.Append("SCOPE_IDENTITY()");
        }
    }

    protected override bool IsIdentityOperation(IColumnModification modification)
        => modification is { IsKey: true, IsRead: true }
            && (modification.Property is null
                || modification.Property.GetDamengValueGenerationStrategy()
                    is DamengValueGenerationStrategy.IdentityColumn
                        or DamengValueGenerationStrategy.Sequence);

    protected override ResultSetMapping AppendSelectAffectedCountCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        int commandPosition)
    {
        commandStringBuilder
            .Append("/*EFCOREROWCOUNT*/SELECT SQL%ROWCOUNT")
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine();

        return ResultSetMapping.LastInResultSet
            | ResultSetMapping.ResultSetWithRowsAffectedOnly;
    }
}
