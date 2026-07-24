using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace W.EntityFrameworkCore.Dameng.Infrastructure.Internal;

/// <summary>
/// Internal configuration state used by the Dameng EF Core provider.
/// </summary>
public sealed class DamengOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    /// Initializes an empty provider options extension.
    /// </summary>
    public DamengOptionsExtension()
    {
    }

    private DamengOptionsExtension(DamengOptionsExtension copyFrom)
        : base(copyFrom)
    {
    }

    /// <inheritdoc />
    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <inheritdoc />
    protected override RelationalOptionsExtension Clone()
        => new DamengOptionsExtension(this);

    /// <inheritdoc />
    public override void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkDameng();

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension)
        : RelationalExtensionInfo(extension)
    {
        private string? _logFragment;

        public override bool IsDatabaseProvider
            => true;

        public override string LogFragment
            => _logFragment ??= new StringBuilder(base.LogFragment)
                .Append("UsingDameng ")
                .ToString();

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["W.EntityFrameworkCore.Dameng"] = "1";
    }
}
