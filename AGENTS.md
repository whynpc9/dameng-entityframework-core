# Repository guidance

## Scope

- Keep `W.EntityFrameworkCore.Dameng` independent of UniWeb, ABP, and
  applications. `UniWeb.Xin.Dameng` belongs in `uniweb-framework`.
- Preserve the pinned EF Core 10 / `DM.DmProvider` ranges and lock files unless
  dependency work is explicitly requested.
- Do not copy or decompile the official Dameng EF provider. Use public EF
  contracts, public Dameng documentation, and black-box tests.
- Do not add reverse-engineering or idempotent-script claims without the
  corresponding implementation and verification.

## Commands

```bash
dotnet restore W.EntityFrameworkCore.Dameng.slnx --locked-mode --disable-parallel
dotnet build W.EntityFrameworkCore.Dameng.slnx --no-restore
dotnet test test/W.EntityFrameworkCore.Dameng.Tests/W.EntityFrameworkCore.Dameng.Tests.csproj --no-build
```

Real database:

```bash
export DAMENG_TEST_CONNECTION_STRING='<complete connection string>'
dotnet test test/W.EntityFrameworkCore.Dameng.FunctionalTests/W.EntityFrameworkCore.Dameng.FunctionalTests.csproj --no-build
dotnet test test/W.EntityFrameworkCore.Dameng.Specification.Tests/W.EntityFrameworkCore.Dameng.Specification.Tests.csproj --no-build
```

Never commit or print the connection string. A skipped database suite is not
release evidence.

## Provider invariants

- Quote identifiers by component and emit `:name` parameters, never `@name`.
- Keep Dameng public model APIs provider-prefixed (`UseDameng...`,
  `GetDameng...`, `SetDameng...`) so multi-provider consumers compile.
- Reject unsupported or lossy shapes explicitly; do not emit another
  database's syntax or silently truncate values.
- Dameng DDL implicitly commits. Generate DDL with the correct transaction
  suppression, use unique test objects, and clean exact objects in `finally`.
- The driver has no provider-specific `DbBatch`; preserve singular modification
  batches until real evidence supports another strategy.
- Treat driver async as synchronous fallback with limited cancellation.

## Evidence

- Update `docs/compatibility.md` when support changes.
- Distinguish unit SQL/metadata evidence from real-server behavior.
- The specification project is a four-test smoke slice, not upstream EF
  relational conformance.
- Add a focused unit test and, when behavior crosses the driver/server boundary,
  a real-database regression before marking a capability verified.
