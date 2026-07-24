using Microsoft.EntityFrameworkCore.Metadata;
using W.EntityFrameworkCore.Dameng.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Dameng-specific metadata extensions for EF Core models.
/// </summary>
public static class DamengModelExtensions
{
    /// <summary>
    /// Gets the default Dameng value generation strategy.
    /// </summary>
    public static DamengValueGenerationStrategy? GetDamengValueGenerationStrategy(
        this IReadOnlyModel model)
        => (DamengValueGenerationStrategy?)model[DamengAnnotationNames.ValueGenerationStrategy];

    /// <summary>
    /// Sets the default Dameng value generation strategy.
    /// </summary>
    public static void SetDamengValueGenerationStrategy(
        this IMutableModel model,
        DamengValueGenerationStrategy? strategy)
        => model.SetOrRemoveAnnotation(DamengAnnotationNames.ValueGenerationStrategy, strategy);

    /// <summary>
    /// Sets the default Dameng value generation strategy.
    /// </summary>
    public static DamengValueGenerationStrategy? SetDamengValueGenerationStrategy(
        this IConventionModel model,
        DamengValueGenerationStrategy? strategy,
        bool fromDataAnnotation = false)
        => (DamengValueGenerationStrategy?)model.SetOrRemoveAnnotation(
            DamengAnnotationNames.ValueGenerationStrategy,
            strategy,
            fromDataAnnotation)?.Value;

    /// <summary>
    /// Gets the default identity seed.
    /// </summary>
    public static long GetDamengIdentitySeed(this IReadOnlyModel model)
        => (long?)model[DamengAnnotationNames.IdentitySeed] ?? 1;

    /// <summary>
    /// Sets the default identity seed.
    /// </summary>
    public static void SetDamengIdentitySeed(this IMutableModel model, long? seed)
        => model.SetOrRemoveAnnotation(DamengAnnotationNames.IdentitySeed, seed);

    /// <summary>
    /// Sets the default identity seed.
    /// </summary>
    public static long? SetDamengIdentitySeed(
        this IConventionModel model,
        long? seed,
        bool fromDataAnnotation = false)
        => (long?)model.SetOrRemoveAnnotation(
            DamengAnnotationNames.IdentitySeed,
            seed,
            fromDataAnnotation)?.Value;

    /// <summary>
    /// Gets the default identity increment.
    /// </summary>
    public static int GetDamengIdentityIncrement(this IReadOnlyModel model)
        => (int?)model[DamengAnnotationNames.IdentityIncrement] ?? 1;

    /// <summary>
    /// Sets the default identity increment.
    /// </summary>
    public static void SetDamengIdentityIncrement(this IMutableModel model, int? increment)
        => model.SetOrRemoveAnnotation(DamengAnnotationNames.IdentityIncrement, increment);

    /// <summary>
    /// Sets the default identity increment.
    /// </summary>
    public static int? SetDamengIdentityIncrement(
        this IConventionModel model,
        int? increment,
        bool fromDataAnnotation = false)
        => (int?)model.SetOrRemoveAnnotation(
            DamengAnnotationNames.IdentityIncrement,
            increment,
            fromDataAnnotation)?.Value;
}
