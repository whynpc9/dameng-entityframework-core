# 达梦功能测试

这些测试面向现有达梦数据库和当前用户模式，不会创建或删除数据库、用户或模式。
每个存储夹具都会创建名称唯一的对象，并在 `finally` 中删除这些精确对象；
由于达梦 DDL 会隐式提交，DDL 清理绝不依赖事务回滚。

仅在进程环境中设置完整连接字符串：

```bash
export DAMENG_TEST_CONNECTION_STRING='<完整的 DM.DmProvider 连接字符串>'
dotnet test test/W.EntityFrameworkCore.Dameng.FunctionalTests/W.EntityFrameworkCore.Dameng.FunctionalTests.csproj
```

缺少 `DAMENG_TEST_CONNECTION_STRING` 时，每项数据库事实测试都会报告为明确的环境跳过。
这适用于离线工作，但不能作为真实数据库或发布证据。

参考环境为达梦 8.1.5.60 和 `DM.DmProvider` 8.3.1.47463。
它只是一个证据点，不构成最低版本保证。绝不能提交、记录或将凭据和连接详情复制到测试数据中。
