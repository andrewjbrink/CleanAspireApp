using CleanAspireApp.Domain.Heroes;
using CleanAspireApp.Domain.Teams;

namespace CleanAspireApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Hero> Heroes { get; }
    DbSet<Team> Teams { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}