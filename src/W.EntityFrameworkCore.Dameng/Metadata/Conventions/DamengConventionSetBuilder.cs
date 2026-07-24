using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace W.EntityFrameworkCore.Dameng.Metadata.Conventions;

internal sealed class DamengConventionSetBuilder : RelationalConventionSetBuilder
{
    internal const int MaxIdentifierLength = 128;

    private readonly ProviderConventionSetBuilderDependencies _dependencies;
    private readonly RelationalConventionSetBuilderDependencies _relationalDependencies;

    public DamengConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();
        conventionSet.Add(
            new RelationalMaxIdentifierLengthConvention(
                MaxIdentifierLength,
                _dependencies,
                _relationalDependencies));
        conventionSet.Add(new DamengValueGenerationStrategyConvention());

        return conventionSet;
    }
}
