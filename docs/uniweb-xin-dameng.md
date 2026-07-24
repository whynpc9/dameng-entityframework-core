# `UniWeb.Xin.Dameng` 集成契约

## 包边界

`UniWeb.Xin.Dameng` 应归属于 `uniweb-framework` 仓库。其最小依赖方向为：

```text
UniWeb.Xin.Dameng
  -> UniWeb.Xin.Abstractions
  -> W.EntityFrameworkCore.Dameng（包）
```

适配器负责选择和配置提供程序。SQL 生成、映射、迁移、重试分类及
`DM.DmProvider` 服务仍由本基础包负责。

新适配器项目应仿照现有 `UniWeb.Xin.SqlServer` / `UniWeb.Xin.PostgreSql`
项目结构，引用 `UniWeb.Xin.Abstractions`，并添加对
`W.EntityFrameworkCore.Dameng` 的包引用。如果聚合包 `UniWeb.Xin`
需要在运行时/设计时发现它，该聚合包还必须引用新项目，以确保
`UniWeb.Xin.Dameng.dll` 进入应用输出目录。

## 基础提供程序 API

稳定的配置接口如下：

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

同时提供泛型 `DbContextOptionsBuilder<TContext>` 重载。

| 选项 | 单位/语义 | 基础提供程序证据 |
| --- | --- | --- |
| `CommandTimeout(int?)` | `DbCommand.CommandTimeout`，单位为秒 | 关系数据库选项传播已通过单元测试 |
| `MigrationsAssembly(string?)` | 包含迁移的程序集 | 关系数据库选项传播已通过单元测试 |
| `EnableRetryOnFailure(...)` | 使用有界重试的保守达梦执行策略 | 配置和错误分类已通过单元测试；没有完整的真实故障注入套件 |

不得将 `DbConnectionProperties.Timeout` 映射到猜测的连接字符串关键字。
在现有 UniWeb 契约中，它映射到 EF 命令超时。

## 配置器精确结构

`DbmsOptionsConfiguratorFactory` 会发现带特性的类型，并通过
`Activator.CreateInstance` 创建实例，因此适配器需要公共无参构造函数。

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

该结构可针对当前基础提供程序编译。如果 `uniweb-framework` 将来为接口启用可空性注解，
应与该接口保持一致地更新实现签名，而不是添加仅适配器使用的重载。

## 模式与命名

适配器不得照搬 PostgreSQL 的 snake_case 约定，也不得硬编码 `dbo`。达梦通常会将
未限定对象解析到已连接用户的模式中。需要其他模式的应用应在 EF 模型中配置，例如使用
`modelBuilder.HasDefaultSchema(...)`，并确保运行时模型与迁移模型使用同一来源。

模式选择会影响 EF 模型缓存。如果没有配套的模型缓存键策略，不得在共享模型上按请求切换模式。

达梦值生成 API 带有提供程序前缀（`UseDamengIdentityColumn`、`UseDamengSequence`），
从而避免 UniWeb 模型程序集同时引用 SQL Server 或 Npgsql 时出现扩展方法歧义。

## 基础提供程序已证明的能力

真实服务器提供程序测试覆盖：

- 常见 UniWeb 标量类型、值转换器、null、GUID、布尔值、Unicode/CJK、日期和
  `decimal(38,20)`；
- 使用当前上下文状态的租户和软删除全局筛选器；
- `AddDbContextPool` 重用，且不会泄漏测试上下文的租户状态；
- 无键 `FromSql` 投影；
- 标识列和序列生成的键；
- 乐观并发；
- 事务和保存点；
- `ExecuteUpdate` 和 `ExecuteDelete`；
- 幂等迁移脚本，包括标识列种子数据；
- 已测试的迁移 DDL、历史记录和锁子集。

这些证据属于基础提供程序。将 `UniWeb.Xin.Dameng` 加入后，以下内容仍需在
`uniweb-framework` 中测试：

- 反射激活以及精确的 `SupportedDbmsTypes == [DbmsType.Dameng]`；
- 通过 `XinService` 完成运行时和 `dotnet ef` 发现；
- 通过聚合项目 `UniWeb.Xin` 实现包/程序集复制行为；
- 从 `DbConnectionProperties` 精确传播命令超时、重试和迁移程序集；
- 真实 UniWeb 拦截器、模型约定、租户实现及应用迁移程序集；
- 与同一解决方案所用 SQL Server/PostgreSQL 提供程序包共存。

当前基础提供程序基线尚未实现反向工程。幂等脚本可用于已测试的迁移范围，保留达梦
DDL 的非事务性语义，并使用 DIsql `/` 块终止符。适配器必须公开这些边界，
而不是通过回退 SQL 或其他提供程序隐藏它们。
