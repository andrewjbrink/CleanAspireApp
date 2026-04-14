using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.SectionalTitle;

public record GetSSUnitValuationQuery(string SchemeName, string Unit)
    : IRequest<IReadOnlyList<SSValuationDto>>
{
    public string ThisSchemeName = SchemeName;
    public string ThisUnit = Unit;
};

internal sealed class GetUnitValuationQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetSSUnitValuationQuery, IReadOnlyList<SSValuationDto>>
{
    public async Task<IReadOnlyList<SSValuationDto>> Handle(GetSSUnitValuationQuery request, CancellationToken cancellationToken)
    {
        string schemeName = request.ThisSchemeName;
        string unit = request.ThisUnit;
        List<PropertyRecord> records = await pv.GetAllSSUnitValuations(schemeName, unit);
        List<SSValuationDto> valuations = new List<SSValuationDto>();


        foreach (PropertyRecord record in records)
        {

            var sales = record.Sales;
            var salesList = new List<SSSalesDto>();
            foreach (var s in sales)
            {
                var saleDto = new SSSalesDto(
                    s.PropertyReference,
                    s.Address,
                    s.Description,
                    s.ErfExtent,
                    s.DwellingExtent,
                    s.SaleDate,
                    s.SalePrice,
                    s.AddressLocator
                    );
                salesList.Add(saleDto);
            }
            var ssDto = new SSValuationDto(
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
               record.IsScheme,
               record.SCHEME_NAME,
               record.SCHEME_NUMBER,
               record.SCHEME_YEAR,
               record.Link,
               DateTime.Today,
               salesList
               );
            valuations.Add(ssDto);
        }
        return valuations;
    }
}
