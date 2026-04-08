using CleanAspireApp.Domain.Heroes;
using CleanAspireApp.Domain.Teams;
using Vogen;

namespace CleanAspireApp.Infrastructure.Persistence.Configuration;

[EfCoreConverter<TeamId>]
[EfCoreConverter<HeroId>]
[EfCoreConverter<MissionId>]
internal sealed partial class VogenEfCoreConverters;