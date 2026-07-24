using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace W.EntityFrameworkCore.Dameng.Tests;

public sealed class DamengValueGenerationTests
{
    [Fact]
    public void GeneratedIntegerKeyUsesIdentityByConvention()
    {
        using var context = new DefaultContext(CreateOptions<DefaultContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultEntity))!
            .FindProperty(nameof(DefaultEntity.Id))!;

        Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            id.GetDamengValueGenerationStrategy());
        Assert.Equal(1, id.GetDamengIdentitySeed());
        Assert.Equal(1, id.GetDamengIdentityIncrement());
    }

    [Fact]
    public void ExplicitIdentityConfigurationPreservesSeedAndIncrement()
    {
        using var context = new IdentityContext(CreateOptions<IdentityContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultEntity))!
            .FindProperty(nameof(DefaultEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            id.GetDamengValueGenerationStrategy());
        Assert.Equal(7, id.GetDamengIdentitySeed());
        Assert.Equal(3, id.GetDamengIdentityIncrement());
        Assert.Null(id.GetDamengSequenceName());
        Assert.Null(id.GetDamengSequenceSchema());
    }

    [Fact]
    public void SequenceConfigurationCreatesSequenceAndProviderMetadata()
    {
        using var context = new SequenceContext(CreateOptions<SequenceContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultEntity))!
            .FindProperty(nameof(DefaultEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.Sequence,
            id.GetDamengValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
        Assert.Equal("EntityIds", id.GetDamengSequenceName());
        Assert.Equal("APP", id.GetDamengSequenceSchema());
        Assert.Null(id.GetDefaultValueSql());
        Assert.NotNull(context.Model.FindSequence("EntityIds", "APP"));
    }

    [Fact]
    public void SequenceWithoutNameUsesHierarchyNameMetadata()
    {
        using var context = new DefaultSequenceContext(CreateOptions<DefaultSequenceContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultEntity))!
            .FindProperty(nameof(DefaultEntity.Id))!;

        Assert.Equal("DefaultEntitySequence", id.GetDamengSequenceName());
        Assert.Null(id.GetDefaultValueSql());
        Assert.NotNull(context.Model.FindSequence("DefaultEntitySequence"));
    }

    [Fact]
    public void NonIntegerKeyDoesNotUseIdentityByConvention()
    {
        using var context = new GuidContext(CreateOptions<GuidContext>());

        var id = context.Model
            .FindEntityType(typeof(GuidEntity))!
            .FindProperty(nameof(GuidEntity.Id))!;

        Assert.NotEqual(
            DamengValueGenerationStrategy.IdentityColumn,
            id.GetDamengValueGenerationStrategy());
    }

    [Fact]
    public void SmallIntegerKeyDoesNotUseIdentityByConvention()
    {
        using var context = new SmallIntegerContext(CreateOptions<SmallIntegerContext>());

        var id = context.Model
            .FindEntityType(typeof(SmallIntegerEntity))!
            .FindProperty(nameof(SmallIntegerEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.None,
            id.GetDamengValueGenerationStrategy());
    }

    [Fact]
    public void ExplicitIdentityRejectsSmallIntegerProperty()
    {
        using var context = new SmallIntegerIdentityContext(
            CreateOptions<SmallIntegerIdentityContext>());

        var exception = Assert.Throws<ArgumentException>(() => _ = context.Model);

        Assert.Contains(nameof(SmallIntegerEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains(
            nameof(DamengValueGenerationStrategy.IdentityColumn),
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SmallIntegerPropertyCanUseSequence()
    {
        using var context = new SmallIntegerSequenceContext(
            CreateOptions<SmallIntegerSequenceContext>());

        var id = context.Model
            .FindEntityType(typeof(SmallIntegerEntity))!
            .FindProperty(nameof(SmallIntegerEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.Sequence,
            id.GetDamengValueGenerationStrategy());
    }

    [Fact]
    public void SharedPrimaryKeyForeignKeyDoesNotUseIdentityByConvention()
    {
        using var context = new SharedPrimaryKeyContext(
            CreateOptions<SharedPrimaryKeyContext>());

        var principalId = context.Model
            .FindEntityType(typeof(SharedPrincipal))!
            .FindProperty(nameof(SharedPrincipal.Id))!;
        var dependentId = context.Model
            .FindEntityType(typeof(SharedDependent))!
            .FindProperty(nameof(SharedDependent.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            principalId.GetDamengValueGenerationStrategy());
        Assert.Equal(
            DamengValueGenerationStrategy.None,
            dependentId.GetDamengValueGenerationStrategy());
    }

    [Fact]
    public void ExplicitIdentityRejectsNonIntegerProperty()
    {
        using var context = new InvalidIdentityContext(CreateOptions<InvalidIdentityContext>());

        var exception = Assert.Throws<ArgumentException>(() => _ = context.Model);

        Assert.Contains(nameof(GuidEntity.Id), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(DamengValueGenerationStrategy.IdentityColumn), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LastExplicitStrategyClearsIncompatibleFacets()
    {
        using var context = new ReconfiguredContext(CreateOptions<ReconfiguredContext>());

        var id = context.Model
            .FindEntityType(typeof(DefaultEntity))!
            .FindProperty(nameof(DefaultEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            id.GetDamengValueGenerationStrategy());
        Assert.Equal(11, id.GetDamengIdentitySeed());
        Assert.Equal(2, id.GetDamengIdentityIncrement());
        Assert.Null(id.GetDamengSequenceName());
        Assert.Null(id.GetDamengSequenceSchema());
        Assert.Null(id.GetDefaultValueSql());
    }

    [Fact]
    public void ConvertedIntegerPropertyCanUseIdentityRegardlessOfBuilderOrder()
    {
        using var context = new ConvertedIdentityContext(CreateOptions<ConvertedIdentityContext>());

        var id = context.Model
            .FindEntityType(typeof(ConvertedEntity))!
            .FindProperty(nameof(ConvertedEntity.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            id.GetDamengValueGenerationStrategy());
        Assert.Equal(typeof(long), id.GetValueConverter()?.ProviderClrType);
    }

    [Fact]
    public void ExplicitStrategyRejectsConflictingStoreGeneration()
    {
        using var context = new ConflictingGenerationContext(
            CreateOptions<ConflictingGenerationContext>());

        var exception = Assert.Throws<InvalidOperationException>(() => _ = context.Model);

        Assert.Contains("default-value SQL", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void IdentityAnnotationsReachTheDesignTimeRelationalColumn()
    {
        using var context = new IdentityContext(CreateOptions<IdentityContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var column = Assert.Single(Assert.Single(model.Tables).Columns);

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            column["Dameng:ValueGenerationStrategy"]);
        Assert.Equal(7L, column["Dameng:IdentitySeed"]);
        Assert.Equal(3, column["Dameng:IdentityIncrement"]);
    }

    [Fact]
    public void SequenceAnnotationsReachTheDesignTimeRelationalColumn()
    {
        using var context = new SequenceContext(CreateOptions<SequenceContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var column = Assert.Single(Assert.Single(model.Tables).Columns);

        Assert.Equal(
            DamengValueGenerationStrategy.Sequence,
            column["Dameng:ValueGenerationStrategy"]);
        Assert.Equal("EntityIds", column["Dameng:SequenceName"]);
        Assert.Equal("APP", column["Dameng:SequenceSchema"]);
        Assert.Null(column.DefaultValueSql);
    }

    [Fact]
    public void TptInheritedKeyUsesIdentityOnlyOnTheRootTable()
    {
        using var context = new TptIdentityContext(CreateOptions<TptIdentityContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var baseId = model.FindTable("TptBases", null)!.FindColumn(nameof(TptBase.Id))!;
        var derivedId = model.FindTable("TptDerived", null)!.FindColumn(nameof(TptBase.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            baseId["Dameng:ValueGenerationStrategy"]);
        Assert.Null(derivedId["Dameng:ValueGenerationStrategy"]);
    }

    [Fact]
    public void TptInheritedKeyUsesSequenceMetadataOnlyOnTheRootTable()
    {
        using var context = new TptSequenceContext(CreateOptions<TptSequenceContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var baseId = model.FindTable("TptBases", null)!.FindColumn(nameof(TptBase.Id))!;
        var derivedId = model.FindTable("TptDerived", null)!.FindColumn(nameof(TptBase.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.Sequence,
            baseId["Dameng:ValueGenerationStrategy"]);
        Assert.Equal("TptIds", baseId["Dameng:SequenceName"]);
        Assert.Null(baseId.DefaultValueSql);
        Assert.Null(derivedId["Dameng:ValueGenerationStrategy"]);
        Assert.Null(derivedId.DefaultValueSql);
    }

    [Fact]
    public void TpcInheritedKeyUsesSharedSequenceMetadataOnEveryConcreteTable()
    {
        using var context = new TpcSequenceContext(CreateOptions<TpcSequenceContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var firstId = model.FindTable("TpcFirsts", "APP")!
            .FindColumn(nameof(TpcBase.Id))!;
        var secondId = model.FindTable("TpcSeconds", "APP")!
            .FindColumn(nameof(TpcBase.Id))!;

        foreach (var column in new[] { firstId, secondId })
        {
            Assert.Equal(
                DamengValueGenerationStrategy.Sequence,
                column["Dameng:ValueGenerationStrategy"]);
            Assert.Equal("TpcIds", column["Dameng:SequenceName"]);
            Assert.Equal("APP", column["Dameng:SequenceSchema"]);
            Assert.Null(column.DefaultValueSql);
        }
    }

    [Fact]
    public void TpcCreateScriptUsesOneSharedSequenceForEveryConcreteTable()
    {
        using var context = new TpcSequenceContext(CreateOptions<TpcSequenceContext>());

        var script = context.Database.GenerateCreateScript();

        Assert.Contains("CREATE SEQUENCE \"APP\".\"TpcIds\"", script, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE \"APP\".\"TpcFirsts\"", script, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE \"APP\".\"TpcSeconds\"", script, StringComparison.Ordinal);
        Assert.Equal(
            2,
            script.Split("\"APP\".\"TpcIds\".NEXTVAL", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void TpcWithOneConcreteTableCanUseIdentity()
    {
        using var context = new SingleTableTpcIdentityContext(
            CreateOptions<SingleTableTpcIdentityContext>());

        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var id = model.FindTable("OnlyTpcTable", null)!
            .FindColumn(nameof(TpcBase.Id))!;

        Assert.Equal(
            DamengValueGenerationStrategy.IdentityColumn,
            id["Dameng:ValueGenerationStrategy"]);
        Assert.Contains(
            "\"Id\" BIGINT IDENTITY(1,1) NOT NULL",
            context.Database.GenerateCreateScript(),
            StringComparison.Ordinal);
    }

    private static DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
        => new DbContextOptionsBuilder<TContext>()
            .UseDameng("Server=localhost;Port=5236;User=test;Password=test")
            .Options;

    private sealed class DefaultContext(DbContextOptions<DefaultContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
    }

    private sealed class IdentityContext(DbContextOptions<IdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn(7, 3);
        }
    }

    private sealed class SequenceContext(DbContextOptions<SequenceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultEntity>().Property(entity => entity.Id)
                .UseDamengSequence("EntityIds", "APP");
        }
    }

    private sealed class DefaultSequenceContext(DbContextOptions<DefaultSequenceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultEntity>().Property(entity => entity.Id)
                .UseDamengSequence();
        }
    }

    private sealed class GuidContext(DbContextOptions<GuidContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<GuidEntity>().HasKey(entity => entity.Id);
    }

    private sealed class SmallIntegerContext(
        DbContextOptions<SmallIntegerContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<SmallIntegerEntity>().HasKey(entity => entity.Id);
    }

    private sealed class SmallIntegerIdentityContext(
        DbContextOptions<SmallIntegerIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SmallIntegerEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<SmallIntegerEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn();
        }
    }

    private sealed class SmallIntegerSequenceContext(
        DbContextOptions<SmallIntegerSequenceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SmallIntegerEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<SmallIntegerEntity>().Property(entity => entity.Id)
                .UseDamengSequence("SmallIntegerIds");
        }
    }

    private sealed class InvalidIdentityContext(DbContextOptions<InvalidIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuidEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<GuidEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn();
        }
    }

    private sealed class SharedPrimaryKeyContext(
        DbContextOptions<SharedPrimaryKeyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SharedPrincipal>().HasKey(entity => entity.Id);
            modelBuilder.Entity<SharedDependent>().HasKey(entity => entity.Id);
            modelBuilder.Entity<SharedPrincipal>()
                .HasOne(entity => entity.Dependent)
                .WithOne(entity => entity.Principal)
                .HasForeignKey<SharedDependent>(entity => entity.Id);
        }
    }

    private sealed class ReconfiguredContext(DbContextOptions<ReconfiguredContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultEntity>().Property(entity => entity.Id)
                .UseDamengSequence("OldSequence", "APP")
                .UseDamengIdentityColumn(11, 2);
        }
    }

    private sealed class ConvertedIdentityContext(DbContextOptions<ConvertedIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertedEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<ConvertedEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn()
                .HasConversion(
                    value => value.Value,
                    value => new ConvertedId(value));
        }
    }

    private sealed class ConflictingGenerationContext(
        DbContextOptions<ConflictingGenerationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<DefaultEntity>().Property(entity => entity.Id)
                .UseDamengIdentityColumn()
                .HasDefaultValueSql("42");
        }
    }

    private sealed class TptIdentityContext(
        DbContextOptions<TptIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TptBase>().UseTptMappingStrategy();
            modelBuilder.Entity<TptBase>().ToTable("TptBases");
            modelBuilder.Entity<TptDerived>().ToTable("TptDerived");
        }
    }

    private sealed class TptSequenceContext(
        DbContextOptions<TptSequenceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TptBase>().UseTptMappingStrategy();
            modelBuilder.Entity<TptBase>().ToTable("TptBases");
            modelBuilder.Entity<TptBase>().Property(entity => entity.Id)
                .UseDamengSequence("TptIds");
            modelBuilder.Entity<TptDerived>().ToTable("TptDerived");
        }
    }

    private sealed class TpcSequenceContext(
        DbContextOptions<TpcSequenceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("APP");
            modelBuilder.Entity<TpcBase>().UseTpcMappingStrategy();
            modelBuilder.Entity<TpcBase>().Property(entity => entity.Id)
                .UseDamengSequence("TpcIds");
            modelBuilder.Entity<TpcFirst>().ToTable("TpcFirsts");
            modelBuilder.Entity<TpcSecond>().ToTable("TpcSeconds");
        }
    }

    private sealed class SingleTableTpcIdentityContext(
        DbContextOptions<SingleTableTpcIdentityContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TpcBase>().UseTpcMappingStrategy();
            modelBuilder.Entity<TpcBase>().Property(entity => entity.Id)
                .UseDamengIdentityColumn();
            modelBuilder.Entity<TpcFirst>().ToTable("OnlyTpcTable");
        }
    }

    private sealed class DefaultEntity
    {
        public long Id { get; set; }
    }

    private sealed class GuidEntity
    {
        public Guid Id { get; set; }
    }

    private sealed class SmallIntegerEntity
    {
        public short Id { get; set; }
    }

    private sealed class ConvertedEntity
    {
        public ConvertedId Id { get; set; }
    }

    private sealed class SharedPrincipal
    {
        public long Id { get; set; }

        public SharedDependent? Dependent { get; set; }
    }

    private sealed class SharedDependent
    {
        public long Id { get; set; }

        public SharedPrincipal Principal { get; set; } = null!;
    }

    private class TptBase
    {
        public long Id { get; set; }
    }

    private sealed class TptDerived : TptBase
    {
        public string? Name { get; set; }
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

    private readonly record struct ConvertedId(long Value);
}
