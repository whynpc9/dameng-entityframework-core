# Provider architecture

## Scope and dependency direction

`W.EntityFrameworkCore.Dameng` is an independently maintained EF Core 10
relational provider built directly on the `DM.DmProvider` ADO.NET driver. The
base package owns database behavior and must not reference UniWeb, ABP, or an
application assembly.

```text
Application / UniWeb.Xin.Dameng
                |
                v
W.EntityFrameworkCore.Dameng
        |                    |
        v                    v
EF Core Relational 10     DM.DmProvider
```

Public configuration exposes EF abstractions and a standard `DbConnection`
overload, not driver-specific connection types. Dameng-specific model APIs use
the `UseDameng...` / `GetDameng...` naming prefix to coexist with SQL Server,
Npgsql, and other provider packages in one consumer.

The provider manages objects in an existing database and user schema. Physical
database creation/deletion is an administrator operation and intentionally
throws through EF's database-creator API.

## EF Core service layers

The provider follows the
[EF Core provider service interfaces](https://learn.microsoft.com/ef/core/providers/writing-a-provider).
The table describes the current baseline, not an aspirational roadmap.

| Layer | Current responsibility and boundary |
| --- | --- |
| Public configuration | Six `UseDameng(...)` overloads plus `DamengDbContextOptionsBuilder` for command timeout, migration assembly, connection ownership, and bounded retry |
| Options and DI | Provider identity, option caching, relational service registration |
| Connection | Creates `DmConnection` from EF connection state or uses the supplied `DbConnection` |
| SQL primitives | Double-quoted component identifiers, `:name` parameters, Dameng statement terminators |
| Type mapping | Common numeric/text/binary/temporal types, Unicode and LOB boundaries, `DateTimeOffset`, interval-backed `TimeSpan`, and `JsonElement` |
| Model conventions | Maximum identifier length plus Dameng identity/sequence annotations, conventions, and design-time annotation generation |
| Query | Pagination, boolean search-condition conversion, selected string/date/GUID translators, and the relational query pipeline |
| Update | Singular modification batches, identity/sequence readback, verified affected-row protocol, concurrency, `ExecuteUpdate`, and `ExecuteDelete` |
| Migrations | Tested Dameng DDL subset, transaction suppression for DDL, seed data, idempotent scripts, history repository, and `DBMS_LOCK` migration lock |
| Database lifecycle | Connection/table existence and schema-object management; physical database create/delete unsupported |
| Design time | Provider/annotation code generation for migrations; reverse engineering is not implemented |

EF provider services use implementation APIs which can change. The package
therefore constrains EF Core Relational to `>= 10.0.10 && < 11.0.0` and locks
resolved dependencies. A successful compile is necessary but not compatibility
evidence.

The repository references EF's relational specification package only for test
utilities. Its specification project currently inherits no upstream
`*TestBase` suites, so it is a smoke slice rather than a conformance suite.

## SQL and model invariants

- Identifiers are double quoted by component. Schema and object names are never
  concatenated before delimiting.
- Parameters use `:name`; provider SQL must not emit `@name`.
- Unicode strings are the default. Chinese identifiers and values are required
  regression cases.
- Sequence access uses `sequence.NEXTVAL`, and generated-key readback uses
  `sequence.CURRVAL`.
- Boolean scalar values and SQL search conditions are converted explicitly
  because Dameng does not accept every EF relational boolean shape unchanged.
- The default schema is not `dbo`. Unqualified objects resolve in the connected
  user's schema unless the model configures another schema.
- Lossy translation is rejected. For example, unsupported fractional
  `DateTime.Add*` arguments are not truncated to an integer.
- Unsupported migration/model shapes fail explicitly instead of emitting SQL
  copied from another database.
- Unbounded text/binary values map to `NCLOB`/`BLOB`; bounded inline mappings
  are required where Dameng needs comparable or indexable values. Declared
  inline length is not a promise that every page-size/row-storage
  configuration can store or index that length.

## Transactions and migrations

Dameng DDL implicitly commits. Generated DDL migration commands therefore
suppress EF transactions, and an entire migration containing DDL is not
atomic. Cleanup tests drop exact objects explicitly; they never rely on
rollback.

DML transaction rollback and savepoint rollback are verified on the reference
server. Savepoints work there despite the current driver's base capability
flag. Supported isolation levels are not yet declared as a provider-wide
contract.

Migration history and `DBMS_LOCK` acquisition/release have real-server tests.
The lock is a provider-wide conservative baseline and requires a server mode
where `DBMS_LOCK` is available; it is not a Dameng MPP guarantee.

Only migration operations named in
[the compatibility matrix](compatibility.md) are claimed. Idempotent scripts
use escaped dynamic SQL inside migration-history guards and DIsql `/` block
terminators; they do not make Dameng DDL transactional. Reverse engineering
remains outside the current baseline.

## ADO.NET boundary

`DM.DmProvider` currently supplies a `net9.0` asset which runs in the tested
.NET 10 process. This is observed compatibility, not a native .NET 10 driver
guarantee.

The driver does not expose a provider-specific `DbBatch`, so modification
commands use EF singular batches. Its async ADO.NET methods fall back to
synchronous implementations in the tested asset, limiting non-blocking I/O and
cancellation even though EF async APIs remain callable.

`CommandTimeout(...)` configures `DbCommand.CommandTimeout` in seconds. This
project does not assert a connection-string timeout keyword or its unit because
that behavior has not been verified for the pinned driver; consumers must use
the exact driver's documentation.

## `UniWeb.Xin.Dameng` boundary

The future adapter belongs in `uniweb-framework`. It selects this provider,
maps UniWeb connection properties to provider options, and participates in
UniWeb reflection discovery. It must not duplicate SQL generation, type
mappings, migrations, retries, or driver services.

Provider runtime tests already exercise important adapter-facing behavior:
pooled-context reset, tenant/soft-delete filters, common scalar/converter
round trips, keyless raw SQL, optimistic concurrency, and bulk DML. Those tests
are evidence for the base provider only; they do not prove reflection discovery
or `dotnet ef` activation of an adapter that has not yet been added to
`uniweb-framework`.

## Clean-room and license boundary

This MIT-licensed provider may use:

- public EF Core contracts and MIT-licensed EF Core source;
- public Dameng SQL and ADO.NET documentation;
- black-box SQL/runtime observations from installed packages and a test
  database;
- third-party providers as public feature checklists when their licenses allow.

It must not copy decompiled source from the official Dameng EF provider,
license-incompatible provider code, or vendor binaries from another ORM.
`DM.DmProvider` remains a separate Apache-2.0 NuGet dependency and is not
embedded. The project does not claim official Dameng support.

Relevant sources:

- [EF Core: Writing a database provider](https://learn.microsoft.com/ef/core/providers/writing-a-provider)
- [EF Core 10 relational package](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Relational/10.0.10)
- [`DM.DmProvider` package](https://www.nuget.org/packages/DM.DmProvider)
- [Dameng .NET programming guide](https://eco.dameng.com/document/dm/zh-cn/pm/net-rogramming-guide.html)
