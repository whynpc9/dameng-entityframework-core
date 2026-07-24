using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Dameng-specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
public static class DamengModelBuilderExtensions
{
    /// <summary>
    /// Configures generated integer properties to use Dameng identity columns by default.
    /// </summary>
    public static ModelBuilder UseDamengIdentityColumns(
        this ModelBuilder modelBuilder,
        long seed = 1,
        int increment = 1)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentOutOfRangeException.ThrowIfZero(increment);

        modelBuilder.Model.SetDamengValueGenerationStrategy(
            DamengValueGenerationStrategy.IdentityColumn);
        modelBuilder.Model.SetDamengIdentitySeed(seed);
        modelBuilder.Model.SetDamengIdentityIncrement(increment);

        return modelBuilder;
    }
}
