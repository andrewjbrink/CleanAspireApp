using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.Tenders;

namespace CleanAspireApp.Application.UseCases.Tenders.Queries;

public record GetTendersQuery() : IRequest<List<EasyTender>>;

internal sealed class GetTendersHandler(ITenderService tenderService) : IRequestHandler<GetTendersQuery, List<EasyTender>>
{
    public async Task<List<EasyTender>> Handle(GetTendersQuery request, CancellationToken cancellationToken)
    {
        var records = await tenderService.GetTendersAsync();
        if (records is not null)
        {
            return records;
        }
        return new List<EasyTender>();

    }
}
