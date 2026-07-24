# W.EntityFrameworkCore.Dameng

An independently maintained, unofficial Entity Framework Core 10 relational
provider for Dameng Database. It uses the official `DM.DmProvider` ADO.NET
driver and has no dependency on UniWeb or ABP.

The current preview baseline targets:

- .NET 10 and Entity Framework Core 10.0.x;
- Dameng DM8; the automated reference server is 8.1.5.60, not a declared
  minimum server version;
- `DM.DmProvider` 8.3.1.47463 through the package's shipped `net9.0` asset;
- an existing database and user schema. Creating or deleting a physical
  database is intentionally outside this provider.

Real-database regressions cover ordinary queries and CRUD, identity and sequence
generated keys, optimistic concurrency, transactions and savepoints,
`ExecuteUpdate` / `ExecuteDelete`, common UniWeb data shapes, migrations
primitives, migration history/locking, Unicode/CJK, and the extended mappings
listed in [the compatibility matrix](docs/compatibility.md).
The final reference-server run passed all 33 provider functional tests and all
4 provider-owned relational smoke tests.

This is not a complete EF Core provider:

- reverse engineering (`dotnet ef dbcontext scaffold`) is not implemented;
- the specification project is a small provider-owned smoke slice using EF
  test utilities; it does not inherit the upstream EF relational suites and is
  not a conformance claim.

## Usage

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseDameng(
        connectionString,
        dameng =>
        {
            dameng.CommandTimeout(30);
            dameng.MigrationsAssembly("App.EntityFrameworkCore.Dameng");
            dameng.EnableRetryOnFailure();
        }));
```

The provider exposes typed and untyped overloads for both connection strings
and existing `DbConnection` instances. The connection overload without an
ownership argument leaves disposal to the caller; the explicit
`contextOwnsConnection` overload is available when EF should own it.

Dameng-specific value generation uses provider-prefixed APIs so a model can
reference this package alongside other EF providers without ambiguous
extension methods:

```csharp
modelBuilder.Entity<Order>()
    .Property(order => order.Id)
    .UseDamengIdentityColumn(seed: 1, increment: 1);

modelBuilder.Entity<AuditEvent>()
    .Property(item => item.Id)
    .UseDamengSequence("AuditEventIds");
```

## Build and test

```bash
dotnet restore W.EntityFrameworkCore.Dameng.slnx --locked-mode --disable-parallel
dotnet build W.EntityFrameworkCore.Dameng.slnx --no-restore
dotnet test test/W.EntityFrameworkCore.Dameng.Tests/W.EntityFrameworkCore.Dameng.Tests.csproj --no-build
```

Real-database tests read exactly one secret environment variable:

```bash
export DAMENG_TEST_CONNECTION_STRING='<complete DM.DmProvider connection string>'
dotnet test test/W.EntityFrameworkCore.Dameng.FunctionalTests/W.EntityFrameworkCore.Dameng.FunctionalTests.csproj --no-build
dotnet test test/W.EntityFrameworkCore.Dameng.Specification.Tests/W.EntityFrameworkCore.Dameng.Specification.Tests.csproj --no-build
```

When the variable is absent, database tests are explicitly skipped. A skipped
run is not real-database or release evidence. Never commit or print the
connection string.

## Important runtime boundaries

- Dameng DDL implicitly commits. A migration containing DDL is not atomic, even
  when EF opened a transaction.
- EF idempotent scripts use Dameng `EXECUTE IMMEDIATE` and migration-history
  guards. They are DIsql-style scripts whose DMSQL blocks end with `/`; custom
  `SqlOperation` text containing a client `/` batch separator is rejected, as
  are escaped dynamic command literals whose UTF-8 representation exceeds
  32767 bytes.
- Unbounded `string` and `byte[]` properties map to `NCLOB` and `BLOB`.
  Configure a bounded maximum length for keys, indexes, ordering/grouping, and
  other operations that require ordinary comparable inline values. The actual
  usable inline row/index length also depends on the database page and row
  storage configuration.
- The current driver has no provider-specific `DbBatch`; EF modification
  commands are deliberately executed as singular batches.
- The driver's async ADO.NET methods fall back to synchronous implementations
  in the tested asset, so EF async APIs do not imply non-blocking network I/O
  or prompt cancellation.
- `CommandTimeout(...)` configures `DbCommand.CommandTimeout` in seconds. No
  driver connection-string timeout keyword or unit is asserted by this
  project; configure connection establishment only from the documentation for
  the exact installed driver.

See:

- [Compatibility and verification](docs/compatibility.md)
- [Provider architecture](docs/architecture.md)
- [`UniWeb.Xin.Dameng` integration contract](docs/uniweb-xin-dameng.md)
- [Third-party notices](THIRD-PARTY-NOTICES.md)
