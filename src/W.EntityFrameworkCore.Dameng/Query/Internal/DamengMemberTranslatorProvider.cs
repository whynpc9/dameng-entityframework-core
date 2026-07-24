using Microsoft.EntityFrameworkCore.Query;

namespace W.EntityFrameworkCore.Dameng.Query.Internal;

/// <summary>
/// Provides Dameng translations for CLR members.
/// </summary>
internal sealed class DamengMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    /// <summary>
    /// Initializes a new member translator provider.
    /// </summary>
    public DamengMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies dependencies)
        : base(dependencies)
        => AddTranslators(
        [
            new DamengDateTimeMemberTranslator(dependencies.SqlExpressionFactory),
            new DamengStringMemberTranslator(dependencies.SqlExpressionFactory)
        ]);
}
