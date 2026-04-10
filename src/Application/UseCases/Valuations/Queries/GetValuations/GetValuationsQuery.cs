using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.GetValuations;

public record GetValuationsQuery(string Erf) : IRequest<IReadOnlyList<ValuationDto>>
{
    public string ErfNumber = Erf;

};

//public record ValuationDto(string PropertyReference,
//    IEnumerable<SalesDto> Sales);

public record ValuationDto(string PageNumber,
                            string PropertyReference,
                            string ErfNumber,
                            string Description,
                            string RatingCategory,
                            string Address,
                            double Extent,
                            string MarketValue,
                            string EffectiveDate,
                            string ExpireyDate,
                            string Erf,
                            string Allotment,
                            string Link
    );



internal sealed class GetValuationsQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetValuationsQuery, IReadOnlyList<ValuationDto>>
{

    public async Task<IReadOnlyList<ValuationDto>> Handle(GetValuationsQuery request, CancellationToken cancellationToken)
    {
        var erf = request.ErfNumber;

        List<PropertyRecord> records = await pv.GetAllValuations(erf);

        List<ValuationDto> valuations = new List<ValuationDto>();

        foreach (PropertyRecord record in records)
        {
            var valuation = new ValuationDto(
                record.PageNumber,
                    record.PropertyReference,
                    record.ErfNumber,
                    record.Description,
                    record.RatingCategory,
                    record.Address,
                    record.Extent,
                    record.MarketValue,
                    record.EffectiveDate,
                    record.DisputeExpiryDate,
                    record.Erf,
                    record.Allotment,
                    record.Link
                );
            valuations.Add(valuation);
        }
        return await Task.FromResult(valuations);
    }
}
