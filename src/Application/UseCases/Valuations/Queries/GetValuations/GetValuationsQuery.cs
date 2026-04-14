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
                            DateTime DateCreated,
                            string Link,
                            IEnumerable<SalesDto> Sales,
                            IEnumerable<HangHoldDto> ValuedTogether
    );

public record SalesDto(string PropertyReference,
    string Address,
    string Description,
    string ErfExtent,
    string DwellingExtent,
    string SaleDate,
    string SalePrice,
    string AddressLocator);

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
                    DateTime.Today,
                    record.Link,
                    salesList,
                    listHanging
                );
            valuations.Add(valuation);
        }
        return await Task.FromResult(valuations);
    }
}
