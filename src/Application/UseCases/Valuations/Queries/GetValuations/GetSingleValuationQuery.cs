using CleanAspireApp.Application.Interfaces;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.GetValuations;

public record GetSingleValuationQuery(string Erf, string Allotment) : IRequest<SingleValuationDto>
{
    public string ThisErf = Erf;
    public string Allotment = Allotment;
};

public record SingleValuationDto(
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
                            string Link,
                            IEnumerable<SalesDto> Sales);

public record SalesDto(string PropertyReference,
    string Address,
    string Description,
    string ErfExtent,
    string DwellingExtent,
    string SaleDate,
    string SalePrice,
    string AddressLocator);

internal sealed class GetSingleValuationQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetSingleValuationQuery, SingleValuationDto>
{
    public async Task<SingleValuationDto> Handle(GetSingleValuationQuery request, CancellationToken cancellationToken)
    {
        var erf = request.Erf;
        var allotment = request.Allotment;

        var record = await pv.GetPropertyValuation(erf, allotment);
        if (record is not null)
        {

            var sales = record.SalesRecords;
            var salesList = new List<SalesDto>();
            foreach (var s in sales)
            {
                var saleDto = new SalesDto(
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

            var thisValuation = new SingleValuationDto(
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
                    salesList
                );
            return await Task.FromResult(thisValuation);
        }
        return null!;
    }
}
