using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.SectionalTitle;

public record GetSSValuationQuery(string SchemeName) : IRequest<IReadOnlyList<SSValuationDto>>
{
    public string ThisSchemeName = SchemeName;
};

public record SSValuationDto(
                            string PageNumber,
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
                            string Link, IEnumerable<SSSalesDto> Sales);

public record SSSalesDto(string PropertyReference,
    string Address,
    string Description,
    string ErfExtent,
    string DwellingExtent,
    string SaleDate,
    string SalePrice,
    string AddressLocator);

internal sealed class GetSSValuationQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetSSValuationQuery, IReadOnlyList<SSValuationDto>>
{
    public async Task<IReadOnlyList<SSValuationDto>> Handle(GetSSValuationQuery request, CancellationToken cancellationToken)
    {
        string schemeName = request.ThisSchemeName;
        List<PropertyRecord> records = await pv.GetAllSSValuations(schemeName);
        List<SSValuationDto> valuations = new List<SSValuationDto>();
        foreach (PropertyRecord record in records)
        {
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
               record.Link,
               new List<SSSalesDto>()
               );
            valuations.Add(ssDto);
        }

        return valuations;
    }
}
