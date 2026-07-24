using Xunit;

namespace W.EntityFrameworkCore.Dameng.Specification.Tests;

internal static class DamengTestEnvironment
{
    public const string ConnectionStringVariable = "DAMENG_TEST_CONNECTION_STRING";

    public const string MissingConnectionStringSkipReason
        = "[environment] Set DAMENG_TEST_CONNECTION_STRING to run tests against an existing Dameng schema.";

    public static string? ConnectionString
        => Environment.GetEnvironmentVariable(ConnectionStringVariable);

    public static string GetRequiredConnectionString()
        => string.IsNullOrWhiteSpace(ConnectionString)
            ? throw new InvalidOperationException(MissingConnectionStringSkipReason)
            : ConnectionString;
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class DamengDatabaseFactAttribute : FactAttribute
{
    public DamengDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(DamengTestEnvironment.ConnectionString))
        {
            Skip = DamengTestEnvironment.MissingConnectionStringSkipReason;
        }
    }
}
