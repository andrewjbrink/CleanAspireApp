using CleanAspireApp.Application.Interfaces;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.GetValuations;

public record GetSingleValuationQuery(string Erf, string Allotment) : IRequest<ValuationDto>
{
    public string ThisErf = Erf;
    public string Allotment = Allotment;
};





internal sealed class GetSingleValuationQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetSingleValuationQuery, ValuationDto>
{
    public async Task<ValuationDto> Handle(GetSingleValuationQuery request, CancellationToken cancellationToken)
    {
        var erf = request.Erf;
        var allotment = request.Allotment;

        var record = await pv.GetPropertyValuation(erf, allotment);
        if (record is not null)
        {

            var sales = record.Sales;
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


            var listHanging = new List<HangHoldDto>();
            var hanging = record.ValuedTogether;
            foreach (var h in hanging)
            {
                var hangHoldDto = new HangHoldDto(
                    h.PropertyReference,
                    h.Description,
                    h.MarketValue,
                    h.RatingCategory,
                    h.Address,
                    h.Link
                    );
                listHanging.Add(hangHoldDto);
            }

            var thisValuation = new ValuationDto(
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
                    DateTime.Today,
                    record.Link,
                    salesList,
                    listHanging
                );
            return await Task.FromResult(thisValuation);
        }
        return null!;
    }
}
