using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace W.EntityFrameworkCore.Dameng.Scaffolding.Internal;

/// <summary>
/// Generates provider configuration calls for scaffolded contexts.
/// </summary>
internal sealed class DamengCodeGenerator(
    ProviderCodeGeneratorDependencies dependencies)
    : ProviderCodeGenerator(dependencies)
{
    private static readonly MethodInfo UseDamengMethod
        = typeof(DamengDbContextOptionsBuilderExtensions).GetRuntimeMethod(
            nameof(DamengDbContextOptionsBuilderExtensions.UseDameng),
            [
                typeof(DbContextOptionsBuilder),
                typeof(string),
                typeof(Action<DamengDbContextOptionsBuilder>)
            ])!;

    /// <inheritdoc />
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            UseDamengMethod,
            providerOptions is null
                ? [connectionString]
                : [connectionString, new NestedClosureCodeFragment("damengOptions", providerOptions)]);
}
