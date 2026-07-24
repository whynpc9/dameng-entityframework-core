using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace W.EntityFrameworkCore.Dameng.Tests;

public sealed class DamengModelValidatorTests
{
    [Fact]
    public void StoreGeneratedKeyWithoutDamengStrategyIsRejected()
    {
        using var context = new DefaultSqlKeyContext(CreateOptions<DefaultSqlKeyContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(DefaultSqlKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("default-value SQL", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(DamengPropertyBuilderExtensions.UseDamengIdentityColumn), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(DamengPropertyBuilderExtensions.UseDamengSequence), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StoreGeneratedKeyUsingDefaultValueIsRejected()
    {
        using var context = new DefaultValueKeyContext(CreateOptions<DefaultValueKeyContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(DefaultValueKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("a default value", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StoreGeneratedKeyUsingComputedColumnIsRejected()
    {
        using var context = new ComputedKeyContext(CreateOptions<ComputedKeyContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(ComputedKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("computed-column SQL", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ClientGeneratedGuidKeyIsAllowed()
    {
        using var context = new ClientGeneratedKeyContext(
            CreateOptions<ClientGeneratedKeyContext>());

        var id = context.Model
            .FindEntityType(typeof(ClientGeneratedKeyEntity))!
            .FindProperty(nameof(ClientGeneratedKeyEntity.Id))!;

        Assert.Null(id.GetDefaultValueSql());
        Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
    }

    [Fact]
    public void KeyWithDefaultSqlButValueGeneratedNeverIsAllowed()
    {
        using var context = new NonGeneratedDefaultSqlKeyContext(
            CreateOptions<NonGeneratedDefaultSqlKeyContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultSqlKeyEntity))!
            .FindProperty(nameof(DefaultSqlKeyEntity.Id))!;

        Assert.Equal("NEWID()", id.GetDefaultValueSql());
        Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
    }

    [Fact]
    public void StandardRowVersionIsRejected()
    {
        using var context = new RowVersionContext(CreateOptions<RowVersionContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(RowVersionEntity.Version), exception.Message, StringComparison.Ordinal);
        Assert.Contains("row version", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("application-managed concurrency token", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TriggerBackedGeneratedConcurrencyTokenIsAllowed()
    {
        using var context = new TriggerConcurrencyContext(
            CreateOptions<TriggerConcurrencyContext>());

        var version = context.Model
            .FindEntityType(typeof(RowVersionEntity))!
            .FindProperty(nameof(RowVersionEntity.Version))!;

        Assert.True(version.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
    }

    [Fact]
    public void MultipleIdentityColumnsInOneTableAreRejected()
    {
        using var context = new MultipleIdentityContext(
            CreateOptions<MultipleIdentityContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("multiple Dameng identity columns", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(MultipleIdentityEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(MultipleIdentityEntity.OtherGeneratedValue), exception.Message, StringComparison.Ordinal);
        Assert.Contains("MultipleIdentityEntities", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void IdentityAcrossMultipleTpcTablesIsRejected()
    {
        using var context = new TpcIdentityContext(CreateOptions<TpcIdentityContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("TPC", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(TpcBase.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("TpcFirsts", exception.Message, StringComparison.Ordinal);
        Assert.Contains("TpcSeconds", exception.Message, StringComparison.Ordinal);
        Assert.Contains(
            nameof(DamengPropertyBuilderExtensions.UseDamengSequence),
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ModelIdentityBuilderRejectsZeroIncrement()
    {
        using var context = new ZeroModelIdentityBuilderContext(
            CreateOptions<ZeroModelIdentityBuilderContext>());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _ = context.Model);

        Assert.Equal("increment", exception.ParamName);
    }

    [Fact]
    public void PropertyIdentityBuilderRejectsZeroIncrement()
    {
        using var context = new ZeroPropertyIdentityBuilderContext(
            CreateOptions<ZeroPropertyIdentityBuilderContext>());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _ = context.Model);

        Assert.Equal("increment", exception.ParamName);
    }

    [Fact]
    public void SnapshotModelIdentityAnnotationWithZeroIncrementIsRejected()
    {
        using var context = new ZeroModelIdentityAnnotationContext(
            CreateOptions<ZeroModelIdentityAnnotationContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("model identity increment", exception.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be zero", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SnapshotPropertyIdentityAnnotationWithZeroIncrementIsRejected()
    {
        using var context = new ZeroPropertyIdentityAnnotationContext(
            CreateOptions<ZeroPropertyIdentityAnnotationContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(DefaultValueKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be zero", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NegativeIdentityIncrementIsAllowed()
    {
        using var context = new NegativeIdentityIncrementContext(
            CreateOptions<NegativeIdentityIncrementContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultValueKeyEntity))!
            .FindProperty(nameof(DefaultValueKeyEntity.Id))!;

        Assert.Equal(-2, id.GetDamengIdentityIncrement());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(39)]
    public void DecimalPrecisionOutsideDamengRangeIsRejected(int precision)
    {
        using var context = precision == 0
            ? (DbContext)new ZeroDecimalPrecisionContext(
                CreateOptions<ZeroDecimalPrecisionContext>())
            : new ExcessiveDecimalPrecisionContext(
                CreateOptions<ExcessiveDecimalPrecisionContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("precision", exception.Message, StringComparison.Ordinal);
        Assert.Contains(
            precision.ToString(System.Globalization.CultureInfo.InvariantCulture),
            exception.Message,
            StringComparison.Ordinal);
        Assert.Contains("1 and 38", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DecimalScaleAbovePrecisionIsRejected()
    {
        using var context = new InvalidDecimalScaleContext(
            CreateOptions<InvalidDecimalScaleContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("scale", exception.Message, StringComparison.Ordinal);
        Assert.Contains("5", exception.Message, StringComparison.Ordinal);
        Assert.Contains("precision 4", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OversizedStringKeyMappedToLobIsRejected()
    {
        using var context = new LobStringKeyContext(CreateOptions<LobStringKeyContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(LobStringKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("32768", exception.Message, StringComparison.Ordinal);
        Assert.Contains("bounded inline store type", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OversizedBinaryIndexMappedToLobIsRejected()
    {
        using var context = new LobBinaryIndexContext(CreateOptions<LobBinaryIndexContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(
            nameof(LobBinaryIndexEntity.Payload),
            exception.Message,
            StringComparison.Ordinal);
        Assert.Contains("32768", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ordinary index", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitLobKeyIsRejected()
    {
        using var context = new ExplicitLobKeyContext(CreateOptions<ExplicitLobKeyContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains(nameof(LobStringKeyEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains("CLOB", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BoundedInlineKeyAndIndexAreAllowed()
    {
        using var context = new BoundedKeyAndIndexContext(
            CreateOptions<BoundedKeyAndIndexContext>());

        var entityType = context.Model.FindEntityType(typeof(BoundedKeyAndIndexEntity))!;

        Assert.Equal(
            "NVARCHAR2(100)",
            entityType.FindProperty(nameof(BoundedKeyAndIndexEntity.Id))!
                .GetRelationalTypeMapping()
                .StoreType);
        Assert.Equal(
            "VARBINARY(100)",
            entityType.FindProperty(nameof(BoundedKeyAndIndexEntity.Payload))!
                .GetRelationalTypeMapping()
                .StoreType);
    }

    private static DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
        => new DbContextOptionsBuilder<TContext>()
            .UseDameng("Server=localhost;Port=5236;User=test;Password=test")
            .Options;

    private sealed class DefaultSqlKeyContext(DbContextOptions<DefaultSqlKeyContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultSqlKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultSqlKeyEntity>().Property(entity => entity.Id)
                .HasDefaultValueSql("NEWID()");
        }
    }

    private sealed class ClientGeneratedKeyContext(
        DbContextOptions<ClientGeneratedKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ClientGeneratedKeyEntity>().HasKey(entity => entity.Id);
    }

    private sealed class NonGeneratedDefaultSqlKeyContext(
        DbContextOptions<NonGeneratedDefaultSqlKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultSqlKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultSqlKeyEntity>().Property(entity => entity.Id)
                .HasDefaultValueSql("NEWID()")
                .ValueGeneratedNever();
        }
    }

    private sealed class DefaultValueKeyContext(
        DbContextOptions<DefaultValueKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultValueKeyEntity>().Property(entity => entity.Id)
                .HasDefaultValue(42L);
        }
    }

    private sealed class ComputedKeyContext(DbContextOptions<ComputedKeyContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComputedKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<ComputedKeyEntity>().Property(entity => entity.Id)
                .HasComputedColumnSql("42");
        }
    }

    private sealed class RowVersionContext(DbContextOptions<RowVersionContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RowVersionEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<RowVersionEntity>().Property(entity => entity.Version)
                .IsRowVersion();
        }
    }

    private sealed class TriggerConcurrencyContext(
        DbContextOptions<TriggerConcurrencyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<RowVersionEntity>();
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Version)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
            entity.ToTable(
                "RowVersionEntities",
                tableBuilder => tableBuilder.HasTrigger("TR_RowVersionEntities_Version"));
        }
    }

    private sealed class MultipleIdentityContext(
        DbContextOptions<MultipleIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<MultipleIdentityEntity>();
            entity.ToTable("MultipleIdentityEntities");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.OtherGeneratedValue)
                .UseDamengIdentityColumn();
        }
    }

    private sealed class TpcIdentityContext(
        DbContextOptions<TpcIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TpcBase>().UseTpcMappingStrategy();
            modelBuilder.Entity<TpcBase>().Property(entity => entity.Id)
                .UseDamengIdentityColumn();
            modelBuilder.Entity<TpcFirst>().ToTable("TpcFirsts");
            modelBuilder.Entity<TpcSecond>().ToTable("TpcSeconds");
        }
    }

    private sealed class ZeroModelIdentityBuilderContext(
        DbContextOptions<ZeroModelIdentityBuilderContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDamengIdentityColumns(increment: 0);
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
        }
    }

    private sealed class ZeroPropertyIdentityBuilderContext(
        DbContextOptions<ZeroPropertyIdentityBuilderContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultValueKeyEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn(increment: 0);
        }
    }

    private sealed class ZeroModelIdentityAnnotationContext(
        DbContextOptions<ZeroModelIdentityAnnotationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Model.SetAnnotation("Dameng:IdentityIncrement", 0);
        }
    }

    private sealed class ZeroPropertyIdentityAnnotationContext(
        DbContextOptions<ZeroPropertyIdentityAnnotationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultValueKeyEntity>().Property(entity => entity.Id)
                .Metadata.SetAnnotation("Dameng:IdentityIncrement", 0);
        }
    }

    private sealed class NegativeIdentityIncrementContext(
        DbContextOptions<NegativeIdentityIncrementContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultValueKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultValueKeyEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn(increment: -2);
        }
    }

    private sealed class ZeroDecimalPrecisionContext(
        DbContextOptions<ZeroDecimalPrecisionContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DecimalEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DecimalEntity>().Property(entity => entity.Amount)
                .HasPrecision(0, 0);
        }
    }

    private sealed class ExcessiveDecimalPrecisionContext(
        DbContextOptions<ExcessiveDecimalPrecisionContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DecimalEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DecimalEntity>().Property(entity => entity.Amount)
                .HasPrecision(39, 0);
        }
    }

    private sealed class InvalidDecimalScaleContext(
        DbContextOptions<InvalidDecimalScaleContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DecimalEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DecimalEntity>().Property(entity => entity.Amount)
                .HasPrecision(4, 5);
        }
    }

    private sealed class LobStringKeyContext(
        DbContextOptions<LobStringKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LobStringKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<LobStringKeyEntity>().Property(entity => entity.Id)
                .HasMaxLength(32768);
        }
    }

    private sealed class LobBinaryIndexContext(
        DbContextOptions<LobBinaryIndexContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LobBinaryIndexEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<LobBinaryIndexEntity>().Property(entity => entity.Payload)
                .HasMaxLength(32768);
            modelBuilder.Entity<LobBinaryIndexEntity>()
                .HasIndex(entity => entity.Payload);
        }
    }

    private sealed class ExplicitLobKeyContext(
        DbContextOptions<ExplicitLobKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LobStringKeyEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<LobStringKeyEntity>().Property(entity => entity.Id)
                .HasColumnType("CLOB");
        }
    }

    private sealed class BoundedKeyAndIndexContext(
        DbContextOptions<BoundedKeyAndIndexContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoundedKeyAndIndexEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<BoundedKeyAndIndexEntity>().Property(entity => entity.Id)
                .HasMaxLength(100);
            modelBuilder.Entity<BoundedKeyAndIndexEntity>().Property(entity => entity.Payload)
                .HasMaxLength(100);
            modelBuilder.Entity<BoundedKeyAndIndexEntity>()
                .HasIndex(entity => entity.Payload);
        }
    }

    private sealed class DefaultSqlKeyEntity
    {
        public Guid Id { get; set; }
    }

    private sealed class ClientGeneratedKeyEntity
    {
        public Guid Id { get; set; }
    }

    private sealed class DefaultValueKeyEntity
    {
        public long Id { get; set; }
    }

    private sealed class ComputedKeyEntity
    {
        public long Id { get; set; }
    }

    private sealed class RowVersionEntity
    {
        public long Id { get; set; }

        public byte[] Version { get; set; } = [];
    }

    private sealed class MultipleIdentityEntity
    {
        public long Id { get; set; }

        public long OtherGeneratedValue { get; set; }
    }

    private sealed class DecimalEntity
    {
        public long Id { get; set; }

        public decimal Amount { get; set; }
    }

    private sealed class LobStringKeyEntity
    {
        public string Id { get; set; } = null!;
    }

    private sealed class LobBinaryIndexEntity
    {
        public long Id { get; set; }

        public byte[] Payload { get; set; } = [];
    }

    private sealed class BoundedKeyAndIndexEntity
    {
        public string Id { get; set; } = null!;

        public byte[] Payload { get; set; } = [];
    }

    private abstract class TpcBase
    {
        public long Id { get; set; }
    }

    private sealed class TpcFirst : TpcBase
    {
        public string? FirstValue { get; set; }
    }

    private sealed class TpcSecond : TpcBase
    {
        public string? SecondValue { get; set; }
    }
}
