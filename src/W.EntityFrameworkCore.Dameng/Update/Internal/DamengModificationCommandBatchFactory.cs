using Microsoft.EntityFrameworkCore.Update;

namespace W.EntityFrameworkCore.Dameng.Update.Internal;

internal sealed class DamengModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private readonly ModificationCommandBatchFactoryDependencies _dependencies;

    public DamengModificationCommandBatchFactory(ModificationCommandBatchFactoryDependencies dependencies)
        => _dependencies = dependencies;

    public ModificationCommandBatch Create()
        => new SingularModificationCommandBatch(_dependencies);
}
