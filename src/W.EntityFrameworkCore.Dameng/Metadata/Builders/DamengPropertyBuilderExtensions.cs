using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Dameng-specific extension methods for property builders.
/// </summary>
public static class DamengPropertyBuilderExtensions
{
    /// <summary>
    /// Configures the property to use a Dameng identity column.
    /// </summary>
    public static PropertyBuilder UseDamengIdentityColumn(
        this PropertyBuilder propertyBuilder,
        long seed = 1,
        int increment = 1)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);
        ArgumentOutOfRangeException.ThrowIfZero(increment);

        var property = propertyBuilder.Metadata;
        property.SetDamengValueGenerationStrategy(
            DamengValueGenerationStrategy.IdentityColumn);
        property.SetDamengIdentitySeed(seed);
        property.SetDamengIdentityIncrement(increment);
        property.SetDamengSequenceName(null);
        property.SetDamengSequenceSchema(null);
        property.SetDefaultValue(null);
        property.SetDefaultValueSql(null);
        property.SetComputedColumnSql(null);
        propertyBuilder.ValueGeneratedOnAdd();

        return propertyBuilder;
    }

    /// <summary>
    /// Configures the property to use a Dameng identity column.
    /// </summary>
    public static PropertyBuilder<TProperty> UseDamengIdentityColumn<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        long seed = 1,
        int increment = 1)
        => (PropertyBuilder<TProperty>)UseDamengIdentityColumn(
            (PropertyBuilder)propertyBuilder,
            seed,
            increment);

    /// <summary>
    /// Configures the property to use a Dameng sequence.
    /// </summary>
    public static PropertyBuilder UseDamengSequence(
        this PropertyBuilder propertyBuilder,
        string? name = null,
        string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        if (name is { Length: 0 })
        {
            throw new ArgumentException("The sequence name cannot be empty.", nameof(name));
        }

        if (schema is { Length: 0 })
        {
            throw new ArgumentException("The sequence schema cannot be empty.", nameof(schema));
        }

        var property = propertyBuilder.Metadata;
        property.SetDamengValueGenerationStrategy(DamengValueGenerationStrategy.Sequence);
        property.SetDamengSequenceName(name);
        property.SetDamengSequenceSchema(schema);
        property.SetDamengIdentitySeed(null);
        property.SetDamengIdentityIncrement(null);
        property.SetDefaultValue(null);
        property.SetDefaultValueSql(null);
        property.SetComputedColumnSql(null);
        propertyBuilder.ValueGeneratedOnAdd();

        return propertyBuilder;
    }

    /// <summary>
    /// Configures the property to use a Dameng sequence.
    /// </summary>
    public static PropertyBuilder<TProperty> UseDamengSequence<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null)
        => (PropertyBuilder<TProperty>)UseDamengSequence(
            (PropertyBuilder)propertyBuilder,
            name,
            schema);
}
