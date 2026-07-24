# Dameng functional tests

These tests target an existing Dameng database and current user schema. They do
not create or drop a database, user, or schema. Each storage fixture creates
uniquely named objects and drops those exact objects in `finally`; DDL cleanup
never relies on transaction rollback because Dameng DDL implicitly commits.

Set the complete connection string only in the process environment:

```bash
export DAMENG_TEST_CONNECTION_STRING='<complete DM.DmProvider connection string>'
dotnet test test/W.EntityFrameworkCore.Dameng.FunctionalTests/W.EntityFrameworkCore.Dameng.FunctionalTests.csproj
```

Without `DAMENG_TEST_CONNECTION_STRING`, every database fact is reported as an
explicit environment skip. That is suitable for offline work but is not
real-database or release evidence.

The reference environment is Dameng 8.1.5.60 with `DM.DmProvider`
8.3.1.47463. It is an evidence point, not a minimum-version guarantee.
Credentials and connection details must never be committed, logged, or copied
into test data.
