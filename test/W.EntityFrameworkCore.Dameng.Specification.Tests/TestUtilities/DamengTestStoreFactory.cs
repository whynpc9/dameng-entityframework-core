using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace W.EntityFrameworkCore.Dameng.Specification.Tests.TestUtilities;

internal sealed class DamengTestStoreFactory : RelationalTestStoreFactory
{
    public static DamengTestStoreFactory Instance { get; } = new();

    private DamengTestStoreFactory()
    {
    }

    public override TestStore Create(string storeName)
        => DamengTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => DamengTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkDameng();
}
