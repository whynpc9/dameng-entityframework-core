using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using W.EntityFrameworkCore.Dameng.Metadata.Internal;

namespace W.EntityFrameworkCore.Dameng.Metadata.Conventions;

internal sealed class DamengValueGenerationStrategyConvention :
    IModelInitializedConvention,
    IModelFinalizingConvention
{
    public void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.Metadata.SetDamengValueGenerationStrategy(
            DamengValueGenerationStrategy.IdentityColumn);

    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                if (!HasTableMapping(entityType))
                {
                    continue;
                }

                var strategy = property.GetDamengValueGenerationStrategy();
                if (strategy == DamengValueGenerationStrategy.None)
                {
                    continue;
                }

                DamengPropertyExtensions.EnsureCompatible(property, strategy);
                EnsureNoConflictingStoreGeneration(property, strategy);

                property.Builder.HasAnnotation(
                    DamengAnnotationNames.ValueGenerationStrategy,
                    strategy);

                if (strategy != DamengValueGenerationStrategy.Sequence)
                {
                    continue;
                }

                var sequenceName = property.GetDamengSequenceName()
                    ?? entityType.GetRootType().ShortName() + "Sequence";
                var sequenceSchema = property.GetDamengSequenceSchema();

                property.Builder.HasAnnotation(
                    DamengAnnotationNames.SequenceName,
                    sequenceName);
                if (sequenceSchema is not null)
                {
                    property.Builder.HasAnnotation(
                        DamengAnnotationNames.SequenceSchema,
                        sequenceSchema);
                }

                modelBuilder.HasSequence(sequenceName, sequenceSchema);
            }
        }
    }

    private static bool HasTableMapping(IReadOnlyEntityType entityType)
        => StoreObjectIdentifier.Create(entityType, StoreObjectType.Table) is not null
            || (entityType.GetMappingStrategy()
                    == RelationalAnnotationNames.TpcMappingStrategy
                && entityType.GetDerivedTypes().Any(
                    derivedType =>
                        StoreObjectIdentifier.Create(
                            derivedType,
                            StoreObjectType.Table) is not null));

    private static void EnsureNoConflictingStoreGeneration(
        IReadOnlyProperty property,
        DamengValueGenerationStrategy strategy)
    {
        var conflict = property.TryGetDefaultValue(out _)
            ? "a default value"
            : property.GetDefaultValueSql() is not null
                ? "default-value SQL"
                : property.GetComputedColumnSql() is not null
                    ? "computed-column SQL"
                    : null;

        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"The property '{property.DeclaringType.DisplayName()}.{property.Name}' cannot use "
                + $"the Dameng '{strategy}' value generation strategy together with {conflict}.");
        }
    }
}
