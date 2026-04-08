using Microsoft.EntityFrameworkCore;
using CleanAspireApp.Application.Common.Interfaces;
using CleanAspireApp.Domain.Common.Interfaces;
using CleanAspireApp.Domain.Heroes;
using CleanAspireApp.Domain.Teams;
using CleanAspireApp.Infrastructure.Persistence.Configuration;
using System.Reflection;

namespace CleanAspireApp.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Hero> Heroes => AggregateRootSet<Hero>();

    public DbSet<Team> Teams => AggregateRootSet<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    private DbSet<T> AggregateRootSet<T>() where T : class, IAggregateRoot => Set<T>();
}