using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using W.EntityFrameworkCore.Dameng.Specification.Tests.TestUtilities;
using Xunit;

namespace W.EntityFrameworkCore.Dameng.Specification.Tests;

public sealed class DamengRelationalSpecificationTests
{
    [Fact]
    [Trait("Category", "ProviderContract")]
    public void EfRelationalSpecificationPackageAndProviderServicesAreWired()
    {
        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Relational.Specification.Tests",
            typeof(RelationalTestStore).Assembly.GetName().Name);

        var services = new ServiceCollection();
        var result = DamengTestStoreFactory.Instance.AddProviderServices(services);

        Assert.Same(services, result);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRelationalConnection));
    }

    [DamengDatabaseFact]
    [Trait("Category", "DamengDatabase")]
    [Trait("SpecificationSlice", "BasicTypes")]
    public Task BasicScalarTypesRoundTrip()
        => WithBasicTypesTableAsync(
            async store =>
            {
                var expected = new SpecificationEntity
                {
                    Id = 101,
                    ByteValue = 42,
                    IntValue = -1_234_567,
                    LongValue = 9_876_543_210,
                    DecimalValue = 123456789.12345678901234567890m,
                    TextValue = "EF Core 10 达梦类型往返",
                    FlagValue = true,
                    GuidValue = Guid.Parse("d570d9ef-8d5d-4c52-9e74-6814719b0b30"),
                    DateValue = new DateOnly(2026, 7, 23),
                    TimestampValue = new DateTime(2026, 7, 23, 10, 11, 12, 345, DateTimeKind.Unspecified)
                };

                await using (var context = CreateContext(store))
                {
                    context.Entities.Add(expected);
                    await context.SaveChangesAsync();
                }

                await using (var context = CreateContext(store))
                {
                    var actual = await context.Entities.SingleAsync(entity => entity.Id == expected.Id);

                    Assert.Equal(expected.ByteValue, actual.ByteValue);
                    Assert.Equal(expected.IntValue, actual.IntValue);
                    Assert.Equal(expected.LongValue, actual.LongValue);
                    Assert.Equal(expected.DecimalValue, actual.DecimalValue);
                    Assert.Equal(expected.TextValue, actual.TextValue);
                    Assert.Equal(expected.FlagValue, actual.FlagValue);
                    Assert.Equal(expected.GuidValue, actual.GuidValue);
                    Assert.Equal(expected.DateValue, actual.DateValue);
                    Assert.Equal(expected.TimestampValue, actual.TimestampValue);
                }
            });

    [DamengDatabaseFact]
    [Trait("Category", "DamengDatabase")]
    [Trait("SpecificationSlice", "Query")]
    public Task ParameterizedFilterOrderAndProjectionExecute()
        => WithBasicTypesTableAsync(
            async store =>
            {
                await using (var context = CreateContext(store))
                {
                    context.Entities.AddRange(
                        CreateEntity(1, 10, "丙"),
                        CreateEntity(2, 30, "甲"),
                        CreateEntity(3, 20, "乙"));
                    await context.SaveChangesAsync();
                }

                const int minimum = 15;

                await using (var context = CreateContext(store))
                {
                    var projection = await context.Entities
                        .Where(entity => entity.IntValue >= minimum)
                        .OrderBy(entity => entity.IntValue)
                        .Select(entity => new { entity.Id, entity.TextValue })
                        .ToListAsync();

                    Assert.Collection(
                        projection,
                        item =>
                        {
                            Assert.Equal(3, item.Id);
                            Assert.Equal("乙", item.TextValue);
                        },
                        item =>
                        {
                            Assert.Equal(2, item.Id);
                            Assert.Equal("甲", item.TextValue);
                        });
                }
            });

    [DamengDatabaseFact]
    [Trait("Category", "DamengDatabase")]
    [Trait("SpecificationSlice", "Update")]
    public Task TrackedUpdateAndDeleteReportExpectedRows()
        => WithBasicTypesTableAsync(
            async store =>
            {
                await using (var context = CreateContext(store))
                {
                    context.Entities.Add(CreateEntity(7, 1, "更新前"));
                    await context.SaveChangesAsync();
                }

                await using (var context = CreateContext(store))
                {
                    var entity = await context.Entities.SingleAsync(item => item.Id == 7);
                    entity.IntValue = 2;
                    entity.TextValue = "更新后";
                    Assert.Equal(1, await context.SaveChangesAsync());
                }

                await using (var context = CreateContext(store))
                {
                    var entity = await context.Entities.SingleAsync(item => item.Id == 7);
                    Assert.Equal(2, entity.IntValue);
                    Assert.Equal("更新后", entity.TextValue);

                    context.Entities.Remove(entity);
                    Assert.Equal(1, await context.SaveChangesAsync());
                }

                await using (var context = CreateContext(store))
                {
                    Assert.False(await context.Entities.AnyAsync(item => item.Id == 7));
                }
            });

    private static async Task WithBasicTypesTableAsync(Func<DamengTestStore, Task> test)
    {
        await using var store = DamengTestStore.Create(nameof(DamengRelationalSpecificationTests));

        try
        {
            await store.CreateBasicTypesTableAsync();
            await test(store);
        }
        finally
        {
            await store.DropObjectsAsync();
        }
    }

    private static SpecificationContext CreateContext(DamengTestStore store)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SpecificationContext>();
        store.AddProviderOptions(optionsBuilder);
        optionsBuilder
            .ReplaceService<IModelCacheKeyFactory, SpecificationModelCacheKeyFactory>()
            .EnableDetailedErrors();

        return new SpecificationContext(optionsBuilder.Options, store.TableName);
    }

    private static SpecificationEntity CreateEntity(long id, int intValue, string text)
        => new()
        {
            Id = id,
            ByteValue = 1,
            IntValue = intValue,
            LongValue = id,
            DecimalValue = intValue,
            TextValue = text,
            FlagValue = id % 2 == 0,
            GuidValue = Guid.NewGuid(),
            DateValue = new DateOnly(2026, 7, 23),
            TimestampValue = new DateTime(2026, 7, 23, 12, 0, 0, DateTimeKind.Unspecified)
        };

    private sealed class SpecificationContext(
        DbContextOptions<SpecificationContext> options,
        string tableName)
        : DbContext(options)
    {
        public string TableName { get; } = tableName;

        public DbSet<SpecificationEntity> Entities
            => Set<SpecificationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<SpecificationEntity>(
                entity =>
                {
                    entity.ToTable(TableName);
                    entity.HasKey(item => item.Id);

                    entity.Property(item => item.Id)
                        .HasColumnName("ID")
                        .ValueGeneratedNever();
                    entity.Property(item => item.ByteValue).HasColumnName("BYTE_VALUE");
                    entity.Property(item => item.IntValue).HasColumnName("INT_VALUE");
                    entity.Property(item => item.LongValue).HasColumnName("LONG_VALUE");
                    entity.Property(item => item.DecimalValue)
                        .HasColumnName("DECIMAL_VALUE")
                        .HasPrecision(38, 20);
                    entity.Property(item => item.TextValue)
                        .HasColumnName("TEXT_VALUE")
                        .HasMaxLength(200)
                        .IsRequired();
                    entity.Property(item => item.FlagValue).HasColumnName("FLAG_VALUE");
                    entity.Property(item => item.GuidValue)
                        .HasColumnName("GUID_VALUE")
                        .HasColumnType("CHAR(36)");
                    entity.Property(item => item.DateValue).HasColumnName("DATE_VALUE");
                    entity.Property(item => item.TimestampValue)
                        .HasColumnName("TIMESTAMP_VALUE")
                        .HasColumnType("TIMESTAMP(6)");
                });
    }

    private sealed class SpecificationModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => context is SpecificationContext specificationContext
                ? (context.GetType(), specificationContext.TableName, designTime)
                : (context.GetType(), designTime);
    }

    private sealed class SpecificationEntity
    {
        public long Id { get; set; }

        public byte ByteValue { get; set; }

        public int IntValue { get; set; }

        public long LongValue { get; set; }

        public decimal DecimalValue { get; set; }

        public required string TextValue { get; set; }

        public bool FlagValue { get; set; }

        public Guid GuidValue { get; set; }

        public DateOnly DateValue { get; set; }

        public DateTime TimestampValue { get; set; }
    }
}
