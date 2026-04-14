using CleanAspireApp.Application.Interfaces;

namespace CleanAspireApp.Application.UseCases.Valuations.Queries.GetValuations;

public record GetFarmValuationQuery(string Farm) : IRequest<FarmValuationDto>
{
    public string ThisFarm = Farm;
};

public record FarmValuationDto(
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
                            DateTime DateCreated,
                            string HoldingLink,
                            IEnumerable<FarmSalesDto> Sales,
                            IEnumerable<HangHoldDto> ValuedTogether);

public record FarmSalesDto(string PropertyReference,
    string Address,
    string Description,
    string ErfExtent,
    string DwellingExtent,
    string SaleDate,
    string SalePrice,
    string AddressLocator);

public record HangHoldDto(
     string PropertyReference,
     string Description,
     string MarketValue,
     string RatingCategory,
     string Address,
     string Link
    );

internal sealed class GetFarmValuationQueryHandler(IPropertyValuation pv)
    : IRequestHandler<GetFarmValuationQuery, FarmValuationDto>
{
    public async Task<FarmValuationDto> Handle(GetFarmValuationQuery request, CancellationToken cancellationToken)
    {
        var farm = request.Farm;
        var record = await pv.GetAllFarmValuations(farm);
        if (record != null)
        {
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

            var sales = record.Sales;
            var salesList = new List<FarmSalesDto>();
            foreach (var s in sales)
            {
                var farmSaleDto = new FarmSalesDto(
                    s.PropertyReference,
                    s.Address,
                    s.Description,
                    s.ErfExtent,
                    s.DwellingExtent,
                    s.SaleDate,
                    s.SalePrice,
                    s.AddressLocator
                    );
                salesList.Add(farmSaleDto);
            }

            var thisValuation = new FarmValuationDto(
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
                       DateTime.Today,
                       record.HoldingLink,
                       salesList,
                       listHanging
                   );
            return await Task.FromResult(thisValuation);
        }
        return null!;
    }
}
