# Compatibility and verification

## Evidence vocabulary

This matrix records the repository baseline as of 2026-07-24.

- **Real verified**: an automated provider test passed against the reference
  Dameng 8.1.5.60 server.
- **Unit verified**: deterministic provider tests cover SQL, metadata, or
  service behavior without a server. This is not runtime proof.
- **Partial**: useful paths are implemented, but the supported EF or Dameng
  surface is deliberately narrower than the area name suggests.
- **Driver-limited**: the provider exposes the EF API, but the current
  `DM.DmProvider` behavior limits its semantics or performance.
- **Not implemented**: callers must not depend on this capability.
- **Unsupported**: the provider intentionally rejects the shape or cannot
  provide the semantics EF expects.

The reference server and driver are an evidence point, not a minimum-version
guarantee. A source file or generated SQL assertion alone does not qualify as
real verification.

## Capability matrix

| Area | State | Verified scope and boundary |
| --- | --- | --- |
| .NET 10 connection | Real verified | `DM.DmProvider` 8.3.1.47463 loads its `net9.0` asset and connects from a .NET 10 process |
| Public configuration | Unit verified | Six `UseDameng(...)` overloads cover typed/untyped builders, connection strings, and caller-owned/EF-owned `DbConnection`; command timeout, migrations assembly, and retry configuration are wired |
| Identifiers and parameters | Real verified | Double-quoted identifiers and `:name` parameters; `@name` is not emitted |
| Scalar/fromless query | Server probe + unit verified | The server accepts `SELECT` without `FROM`; provider SQL generation and boolean value/search-condition conversion have deterministic tests |
| Filtering and pagination | Real verified | Parameterized predicates, ordering, `Skip`/`Take`, `OFFSET … FETCH`, `Any`, tenant and soft-delete filters |
| String translation | Partial / Real verified subset | Real execution covers `Trim`, `Length`, `Contains`, `StartsWith`, and `EndsWith`; generated SQL for `IndexOf`, `Replace`, case conversion, substring, and other trim shapes has unit coverage |
| Date/time and GUID translation | Partial / Real verified subset | Real execution covers year/month extraction, integral `AddDays`, and `Guid.NewGuid()`/`NEWID()`; other implemented members and integral `DATEADD` units have generated-SQL tests |
| Fractional `DateTime.Add*` | Unsupported where lossless translation is unavailable | Parameterized or non-integral double arguments are not silently truncated; EF reports an untranslated expression |
| Tracked CRUD | Real verified | Unicode insert/read/update/delete, nulls, converters, identity key readback, and affected-row reporting |
| Identity columns | Real verified / Unit verified facets | Generated keys and `SCOPE_IDENTITY()` readback execute on the server; conventions and explicit seed/increment have unit coverage |
| Sequences | Real verified | `sequence.NEXTVAL` defaults and `sequence.CURRVAL` generated-key readback; standard `NEXT VALUE FOR` is not used |
| Optimistic concurrency | Real verified | A stale concurrency token throws through the driver's verified `SQL%ROWCOUNT` result protocol |
| `ExecuteUpdate` / `ExecuteDelete` | Real verified | Affected-row counts and persisted boolean/converted-value updates |
| Modification batching | Driver-limited | The driver has no provider-specific `DbBatch`; the provider uses `SingularModificationCommandBatch` |
| Common scalar mappings | Real verified | Signed integers, facet-free decimal plus decimal(38,20), bool, GUID, Unicode/CJK, nullable values, `DateOnly`, microsecond `TimeOnly`, and precision-7 `DateTime` |
| `DateTimeOffset` | Real verified | `DATETIME(7) WITH TIME ZONE`; precision-7 round trip and provider text readback preserve the original offset, including `+08:00` |
| `TimeSpan` | Real verified | `INTERVAL DAY(9) TO SECOND(6)`, including exact positive and negative values beyond two-digit days; literals preserve the mapped day/fractional precision and lossy ticks are rejected |
| Binary and LOB mappings | Real verified / Partial query semantics | `VARBINARY`, 40 KiB `BLOB`, and 40 KiB Unicode `NCLOB` round trips. Parameter equality through `TEXT_EQUAL`/`BLOB_EQUAL` and NCLOB `INSTR` search execute on the reference server; ordering, grouping, distinct, and distinct set operations fail early. String-search functions remain subject to `CLOB_MAX_CALC_LEN`. Keys/indexes require bounded inline types, and page/row configuration controls usable inline length |
| JSON storage | Real verified / Partial | `JsonElement` round trips through `JSON`; a broad JSON query/operator translation surface is not claimed |
| Unsigned integers and facet boundaries | Unit verified | Range-preserving converters and store mappings are covered; broad real-server boundary data is not yet a release claim |
| Transactions | Real verified | EF transaction commit/rollback behavior is exercised |
| Savepoints | Real verified | Create and rollback-to-savepoint work on the reference driver/server despite the driver's base capability flag |
| Isolation levels | Partial | Server probes established a narrower accepted set, but the provider does not yet expose a validated isolation-level compatibility contract |
| Retry execution strategy | Unit verified | Conservative `DmException.Number` classification and bounded settings; no real fault-injection suite currently proves recovery for every code |
| DDL transactionality | Unsupported as an atomic migration | Dameng DDL implicitly commits; generated DDL commands suppress EF transactions |
| Physical database create/delete | Unsupported | `Create`/`Delete` throw; connect to an existing database and manage objects in the current schema |
| Migrations DDL | Partial / Real verified subset | Real tests cover table, identity, sequence, index, virtual computed column, and cleanup; unit tests cover seed-data SQL, additional relational constraints, and explicit rejection paths |
| Migration history | Real verified | Existence, create-if-not-exists, insert, query, and delete paths |
| Migration locking | Real verified / server-specific | Uses `DBMS_LOCK`; verified on the reference non-MPP server. Dameng MPP does not provide the same baseline |
| Idempotent migration scripts | Real verified / Partial | Generated commands use escaped `EXECUTE IMMEDIATE` inside migration-history DMSQL guards. Repeated execution, Unicode, quotes, identity seed/session commands, and history insertion pass on the reference server. Dynamic command literals whose escaped UTF-8 representation exceeds 32767 bytes fail early and must be split |
| Stored computed columns | Unsupported | Virtual computed columns are supported; stored computed columns are rejected |
| Filtered indexes | Unsupported | Provider rejects a migration index filter instead of emitting another database's syntax |
| Identity alteration | Unsupported | Adding/removing `IDENTITY`, or changing its seed/increment with `ALTER COLUMN`, is rejected; recreate the column |
| Cross-schema rename | Unsupported | Table/sequence rename cannot be used to move an object between schemas |
| TPT/TPC value generation | Unit verified / Partial | TPT applies identity/sequence generation only to the root table. TPC identity across multiple concrete tables is rejected because it can collide; a shared Dameng sequence is supported instead |
| Schema creation and uncommon DDL | Partial | Only operations represented in provider tests are claimed; do not infer coverage for every EF migration operation |
| Design-time migration code generation | Unit verified / Partial | Provider and annotation code generators emit `UseDameng` and Dameng value-generation APIs; end-to-end `dotnet ef` coverage remains limited |
| Reverse engineering | Not implemented | No `IDatabaseModelFactory`; `dotnet ef dbcontext scaffold` is outside the current provider surface |
| EF relational specification conformance | Not claimed | The specification project contains four provider-owned smoke tests and uses EF test utilities, but inherits no upstream relational `*TestBase` suite |
| Trimming / NativeAOT | Unverified | No trimming, compiled-model optimization, or NativeAOT compatibility claim is made for the provider or the current driver |
| Async I/O and cancellation | Driver-limited | The tested driver asset falls back to synchronous ADO.NET implementations; EF async APIs remain available but cannot promise non-blocking I/O or prompt cancellation |
| Connection timeout | Unverified driver setting | EF `CommandTimeout` is seconds. This repository does not assert a driver connection-string timeout keyword or unit |

Final 2026-07-24 validation snapshot:

- locked restore: succeeded;
- Release build: 0 warnings, 0 errors;
- deterministic unit suite: 165/165 passed;
- reference-server functional suite: 33/33 passed;
- provider-owned relational smoke suite: 4/4 passed;
- standard `dotnet format --verify-no-changes`: passed;
- Release package creation: passed.

## Real-database test contract

Real-database tests receive one secret environment variable:

```text
DAMENG_TEST_CONNECTION_STRING
```

It contains the complete `DM.DmProvider` connection string. Tests and logs must
not print, snapshot, or commit it. The configured account must be able to
create/drop tables, indexes, constraints, and sequences in its own schema,
execute DML, use transactions/savepoints, access migration history, and invoke
`DBMS_LOCK`. It does not need permission to create or delete a physical
database.

Test isolation rules:

1. Generate a unique, recognizable prefix for every object set.
2. Restrict discovery and cleanup to that prefix and current user schema.
3. Drop exact objects in `finally`, including after assertion failure.
4. Never rely on rollback for DDL cleanup because DDL implicitly commits.
5. Do not persist host names, usernames, passwords, or a complete connection
   string in test output.

Run the database projects directly:

```bash
export DAMENG_TEST_CONNECTION_STRING='<complete connection string>'

dotnet test \
  test/W.EntityFrameworkCore.Dameng.FunctionalTests/W.EntityFrameworkCore.Dameng.FunctionalTests.csproj

dotnet test \
  test/W.EntityFrameworkCore.Dameng.Specification.Tests/W.EntityFrameworkCore.Dameng.Specification.Tests.csproj
```

Without the variable, database facts are explicitly skipped. Skips are useful
for offline development but are not release evidence.

## Specification-test boundary

`W.EntityFrameworkCore.Dameng.Specification.Tests` references
`Microsoft.EntityFrameworkCore.Relational.Specification.Tests` for shared test
utilities. Its current tests are provider-owned smoke scenarios for:

- provider/test-store wiring;
- scalar round trips;
- parameterized query/order/projection;
- tracked update/delete affected rows.

No upstream EF relational base suite is inherited. Therefore “the
specification project passed” means only this narrow slice passed; it must not
be reported as EF Core relational conformance.

## Dameng references

- [DML, `RETURNING`, and `MERGE`](https://eco.dameng.com/document/dm/zh-cn/pm/insertion-deletion-modification.html)
- [Query clauses and pagination](https://eco.dameng.com/document/dm/zh-cn/pm/check-phrases.html)
- [DDL and sequences](https://eco.dameng.com/document/dm/zh-cn/pm/definition-statement.html)
- [Transactions](https://eco.dameng.com/document/dm/zh-cn/pm/management-affairs.html)
- [`DBMS_LOCK`](https://eco.dameng.com/document/dm/zh-cn/pm/dbms_lock-package.html)
- [JSON](https://eco.dameng.com/document/dm/zh-cn/pm/json)
