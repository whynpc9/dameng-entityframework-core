using System.Reflection;
using Dm;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace W.EntityFrameworkCore.Dameng.Tests;

public sealed class DamengExecutionStrategyTests
{
    [Fact]
    public void EnableRetryOnFailureUsesDefaultDamengStrategy()
    {
        using var context = CreateContext(options => options.EnableRetryOnFailure());

        var strategy = Assert.IsType<DamengRetryingExecutionStrategy>(
            context.Database.CreateExecutionStrategy());

        Assert.Equal(6, strategy.MaxRetryCount);
        Assert.Equal(TimeSpan.FromSeconds(30), strategy.MaxRetryDelay);
        Assert.True(strategy.RetriesOnFailure);
    }

    [Fact]
    public void EnableRetryOnFailureAppliesCustomSettingsAndCopiesErrorNumbers()
    {
        var additionalErrors = new List<int> { -12345 };
        var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
        optionsBuilder.UseDameng(
            "Server=localhost;Port=5236;User Id=test;PWD=test",
            options => options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(4),
                additionalErrors));

        additionalErrors.Add(-54321);
        using var context = new TestContext(optionsBuilder.Options);
        var strategy = Assert.IsType<DamengRetryingExecutionStrategy>(
            context.Database.CreateExecutionStrategy());

        Assert.Equal(3, strategy.MaxRetryCount);
        Assert.Equal(TimeSpan.FromSeconds(4), strategy.MaxRetryDelay);
        Assert.Equal([-12345], strategy.AdditionalErrorNumbers);
        Assert.False(
            strategy.AdditionalErrorNumbers
                is ICollection<int> { IsReadOnly: false });
    }

    [Theory]
    [InlineData(-3003)]
    [InlineData(-3404)]
    [InlineData(-6003)]
    [InlineData(-6004)]
    [InlineData(-6010)]
    [InlineData(6001)]
    [InlineData(6027)]
    [InlineData(6060)]
    [InlineData(6089)]
    [InlineData(6123)]
    public void StrategyRecognizesOnlyExplicitDefaultTransientDamengCodes(int errorNumber)
    {
        using var context = CreateContext();
        var strategy = new TestDamengRetryingExecutionStrategy(context);

        Assert.True(strategy.TestShouldRetryOn(CreateDmException(errorNumber)));
    }

    [Theory]
    [InlineData(-2007)] // SQL syntax error
    [InlineData(-1040)] // Invalid connection string
    [InlineData(-1210)] // User locked
    [InlineData(-3002)] // Lock failure is intentionally not classified without stronger semantics
    [InlineData(-6011)] // Unknown host generally indicates configuration/DNS failure
    public void StrategyDoesNotRetryNonTransientOrAmbiguousDamengCodes(int errorNumber)
    {
        using var context = CreateContext();
        var strategy = new TestDamengRetryingExecutionStrategy(context);

        Assert.False(strategy.TestShouldRetryOn(CreateDmException(errorNumber)));
    }

    [Fact]
    public void StrategyDoesNotRetryNonDamengTimeoutException()
    {
        using var context = CreateContext();
        var strategy = new TestDamengRetryingExecutionStrategy(context);

        Assert.False(strategy.TestShouldRetryOn(new TimeoutException()));
    }

    [Fact]
    public void StrategyRetriesAnAdditionalErrorNumber()
    {
        using var context = CreateContext();
        var strategy = new TestDamengRetryingExecutionStrategy(
            context,
            maxRetryCount: 2,
            maxRetryDelay: TimeSpan.Zero,
            [-77777]);
        var attempts = 0;

        var result = strategy.Execute(
            state: 42,
            (_, state) =>
            {
                if (++attempts < 3)
                {
                    throw CreateDmException(-77777);
                }

                return state;
            },
            verifySucceeded: null);

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void CollectionOverloadRejectsNull()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestContext>();

        Assert.Throws<ArgumentNullException>(
            () => optionsBuilder.UseDameng(
                "Server=localhost;Port=5236;User Id=test;PWD=test",
                options => options.EnableRetryOnFailure((ICollection<int>)null!)));
    }

    private static TestContext CreateContext(
        Action<Microsoft.EntityFrameworkCore.Infrastructure.DamengDbContextOptionsBuilder>? configure = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
        optionsBuilder.UseDameng(
            "Server=localhost;Port=5236;User Id=test;PWD=test",
            configure);

        return new TestContext(optionsBuilder.Options);
    }

    private static DmException CreateDmException(int errorNumber)
    {
        var errorConstructor = typeof(DmError).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(int), typeof(string)],
            modifiers: null)!;
        var exceptionConstructor = typeof(DmException).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(DmError)],
            modifiers: null)!;
        var error = (DmError)errorConstructor.Invoke([errorNumber, $"DM error {errorNumber}"]);

        return (DmException)exceptionConstructor.Invoke([error]);
    }

    private sealed class TestContext(DbContextOptions<TestContext> options)
        : DbContext(options);

    private sealed class TestDamengRetryingExecutionStrategy : DamengRetryingExecutionStrategy
    {
        public TestDamengRetryingExecutionStrategy(DbContext context)
            : base(context)
        {
        }

        public TestDamengRetryingExecutionStrategy(
            DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            IEnumerable<int> errorNumbersToAdd)
            : base(context, maxRetryCount, maxRetryDelay, errorNumbersToAdd)
        {
        }

        public bool TestShouldRetryOn(Exception exception)
            => ShouldRetryOn(exception);
    }
}
