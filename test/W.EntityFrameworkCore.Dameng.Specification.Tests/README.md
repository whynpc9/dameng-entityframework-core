# EF Core relational specification smoke slice

This project references
`Microsoft.EntityFrameworkCore.Relational.Specification.Tests` 10.0.x for
shared test utilities, including `RelationalTestStore`. It currently contains
four provider-owned tests:

- provider/test-store service wiring;
- scalar type round trips;
- parameterized filtering, ordering, and projection;
- tracked update/delete affected rows.

It does **not** inherit an upstream EF relational `*TestBase` suite. Passing
this project proves only the smoke slice above; it is not an EF Core relational
conformance result.

The live tests target an existing database and current user schema:

```bash
export DAMENG_TEST_CONNECTION_STRING='<complete DM.DmProvider connection string>'
dotnet test test/W.EntityFrameworkCore.Dameng.Specification.Tests/W.EntityFrameworkCore.Dameng.Specification.Tests.csproj
```

No test creates or deletes a database, user, or schema. Each live test creates
a uniquely named table and drops only that table in `finally`. The connection
string is read from the process environment and must never be logged.

## Skip classification

- `[environment]`: `DAMENG_TEST_CONNECTION_STRING` is absent.
- `[unsupported]`: Dameng cannot provide the required database capability.
- `[provider-gap]`: the provider has not implemented the required surface.

A provider gap must not be classified as a database limitation. Broad upstream
base suites should be adopted only as explicit, reviewable slices with
provider-specific failures and skips accounted for individually.
