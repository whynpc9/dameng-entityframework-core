# `UniWeb.Xin.Dameng` integration contract

## Package boundary

`UniWeb.Xin.Dameng` belongs in the `uniweb-framework` repository. Its minimum
dependency direction is:

```text
UniWeb.Xin.Dameng
  -> UniWeb.Xin.Abstractions
  -> W.EntityFrameworkCore.Dameng (package)
```

The adapter selects and configures the provider. SQL generation, mappings,
migrations, retry classification, and `DM.DmProvider` services remain in this
base package.

The new adapter project should mirror the existing `UniWeb.Xin.SqlServer` /
`UniWeb.Xin.PostgreSql` project shape, reference
`UniWeb.Xin.Abstractions`, and add a package reference to
`W.EntityFrameworkCore.Dameng`. If the aggregate `UniWeb.Xin` package must
discover it at runtime/design time, that aggregate must also reference the new
project so `UniWeb.Xin.Dameng.dll` reaches the application output directory.

## Base-provider API

The stable configuration surface is:

```csharp
DbContextOptionsBuilder UseDameng(
    string? connectionString,
    Action<DamengDbContextOptionsBuilder>? optionsAction = null);

DbContextOptionsBuilder UseDameng(
    DbConnection connection,
    Action<DamengDbContextOptionsBuilder>? optionsAction = null);

DbContextOptionsBuilder UseDameng(
    DbConnection connection,
    bool contextOwnsConnection,
    Action<DamengDbContextOptionsBuilder>? optionsAction = null);
```

Typed `DbContextOptionsBuilder<TContext>` overloads are available as well.

| Option | Unit/semantics | Base-provider evidence |
| --- | --- | --- |
| `CommandTimeout(int?)` | `DbCommand.CommandTimeout`, seconds | Relational option propagation unit tested |
| `MigrationsAssembly(string?)` | Assembly containing migrations | Relational option propagation unit tested |
| `EnableRetryOnFailure(...)` | Conservative Dameng execution strategy with bounded retry | Configuration and error classification unit tested; no complete real fault-injection suite |

Do not map `DbConnectionProperties.Timeout` to a guessed connection-string
keyword. In the existing UniWeb contract it maps to EF command timeout.

## Exact configurator shape

`DbmsOptionsConfiguratorFactory` discovers attributed types and creates them
with `Activator.CreateInstance`, so the adapter needs a public parameterless
constructor.

```csharp
using Microsoft.EntityFrameworkCore;
using UniWeb.Data;
using UniWeb.Xin;

namespace UniWeb.EntityFrameworkCore;

[XinService(DbmsType.Dameng)]
public sealed class DamengOptionsConfigurator : IDbmsOptionsConfigurator
{
    public DbmsType[] SupportedDbmsTypes => [DbmsType.Dameng];

    public void Configure(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        DbConnectionProperties properties,
        string migrationAssemblyName = null)
    {
        optionsBuilder.UseDameng(
            connectionString,
            dameng =>
            {
                dameng.CommandTimeout(properties.Timeout);

                if (properties.EnableRetryOnFailure)
                {
                    dameng.EnableRetryOnFailure();
                }

                if (!string.IsNullOrWhiteSpace(migrationAssemblyName))
                {
                    dameng.MigrationsAssembly(migrationAssemblyName);
                }
            });
    }
}
```

This shape compiles against the current base provider. If
`uniweb-framework` later enables nullable annotations for the interface, update
the implementation signature consistently with that interface rather than
adding an adapter-only overload.

## Schema and naming

The adapter must not apply PostgreSQL's snake-case convention by imitation and
must not hardcode `dbo`. Dameng normally resolves unqualified objects in the
connected user's schema. Applications requiring another schema configure it in
the EF model, for example with `modelBuilder.HasDefaultSchema(...)`, using the
same source for runtime and migration models.

Schema selection affects EF model caching. Do not switch schema per request on
a shared model without a matching model-cache key strategy.

Dameng value-generation APIs are provider-prefixed
(`UseDamengIdentityColumn`, `UseDamengSequence`) to avoid ambiguous extensions
when a UniWeb model assembly also references SQL Server or Npgsql.

## What the base provider already proves

Real-server provider tests cover:

- common UniWeb scalar types, value converters, nulls, GUIDs, booleans,
  Unicode/CJK, dates, and `decimal(38,20)`;
- tenant and soft-delete global filters using current context state;
- `AddDbContextPool` reuse without leaking the test context's tenant state;
- keyless `FromSql` projection;
- identity and sequence generated keys;
- optimistic concurrency;
- transactions and savepoints;
- `ExecuteUpdate` and `ExecuteDelete`;
- idempotent migration scripts, including identity seed data;
- a tested subset of migration DDL, history, and locking.

This evidence belongs to the base provider. The following still requires tests
in `uniweb-framework` after `UniWeb.Xin.Dameng` is added:

- reflection activation and exact
  `SupportedDbmsTypes == [DbmsType.Dameng]`;
- runtime and `dotnet ef` discovery through `XinService`;
- package/assembly-copy behavior through the aggregate `UniWeb.Xin` project;
- exact command-timeout, retry, and migration-assembly propagation from
  `DbConnectionProperties`;
- the real UniWeb interceptors, model conventions, tenant implementation, and
  application migration assembly;
- coexistence with the SQL Server/PostgreSQL provider packages used by the
  same solution.

Reverse engineering is not implemented in the current base-provider baseline.
Idempotent scripts are available for the tested migration surface, retain
Dameng's non-transactional DDL semantics, and use DIsql `/` block terminators.
The adapter must expose those boundaries rather than hiding them with fallback
SQL or another provider.
