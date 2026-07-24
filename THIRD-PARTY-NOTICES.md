# Third-party notices

This project is an independently maintained, unofficial Entity Framework Core
provider. It is not affiliated with, sponsored by, or endorsed by Dameng or
Microsoft.

Runtime dependencies:

- `Microsoft.EntityFrameworkCore.Relational` is licensed under the MIT License.
- `DM.DmProvider` is distributed by Dameng under Apache License 2.0. The driver
  remains a separate NuGet dependency and is not embedded or redistributed by
  this package.

Implementation references:

- The provider service layout follows the public provider extension points and
  MIT-licensed provider examples in the `dotnet/efcore` repository, pinned to
  EF Core 10.0.10 for API compatibility.
- FreeSql's MIT-licensed Dameng provider and SqlSugar's public Dameng behavior
  were used only as feature checklists. Their implementation code is not
  included.

Dameng database server software, installation media, and license keys are not
part of this project.
