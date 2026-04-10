
using CleanAspireApp.Application.UseCases.Valuations.Queries.GetValuations;
using CleanAspireApp.Application.UseCases.Valuations.Queries.SectionalTitle;
using CleanAspireApp.WebApi.Extensions;
using MediatR;

namespace CleanAspireApp.WebApi.Endpoints;

public static class ValuationEndPoints
{
    public static void MapValuationEndpoints(this WebApplication app)
    {
        var group = app.MapApiGroup("valuations");

        group.MapGet("/{erf}", async (string erf, ISender sender, CancellationToken ct) =>
        {
            var results = await sender.Send(new GetValuationsQuery(erf), ct);
            return TypedResults.Ok(results);
        })
         .WithName(nameof(GetValuationsQuery))
         .Produces<ValuationDto[]>()
         .WithSummary("Get All Parcels")
         .WithDescription("Retrieves all the properties for a given Erf Number in all the Allotments.");

        group.MapGet("/ErfValuation/{erf}/{allotment}", async (string erf, string allotment, ISender sender, CancellationToken ct) =>
        {
            var results = await sender.Send(new GetSingleValuationQuery(erf, allotment), ct);
            return TypedResults.Ok(results);
        })
        .WithName(nameof(GetSingleValuationQuery))
        .Produces<ValuationDto[]>()
        .WithSummary("Get Parcel by Erf and Allotment")
        .WithDescription("Retrieves the Valuation for the property in the provided Allotment");

        group.MapGet("/FarmValuation/{farm}", async (string farm, ISender sender, CancellationToken ct) =>
        {
            var results = await sender.Send(new GetFarmValuationQuery(farm), ct);
            return TypedResults.Ok(results);
        })
        .WithName(nameof(GetFarmValuationQuery))
        .Produces<ValuationDto[]>()
        .WithSummary("Get Farm Valuation")
        .WithDescription("Retrieves a farm valuation.");


        group.MapGet("/SchemeValuation/{schemeName}", async (string schemeName, ISender sender, CancellationToken ct) =>
        {
            var results = await sender.Send(new GetSSValuationQuery(schemeName), ct);
            return TypedResults.Ok(results);
        })
       .WithName(nameof(GetSSValuationQuery))
       .Produces<ValuationDto[]>()
       .WithSummary("Get Scheme Valuation")
       .WithDescription("Retrieves all the units for a given Scheme.");


        group.MapGet("/SchemeValuation/{schemeName}/{unit}", async (string schemeName, string unit, ISender sender, CancellationToken ct) =>
        {
            var results = await sender.Send(new GetSSUnitValuationQuery(schemeName, unit), ct);
            return TypedResults.Ok(results);
        })
       .WithName(nameof(GetSSUnitValuationQuery))
       .Produces<ValuationDto[]>()
       .WithSummary("Get Scheme Unit Valuation")
       .WithDescription("Retrieves the valuations for the Scheme and Unit.");




        //app.MapPost("/push-sales", async (
        //    IHubContext<SalesHub> hub,
        //    object sales) =>
        //{
        //    await hub.Clients.All.SendAsync("ReceiveSales", sales);
        //    return Results.Ok();
        //});
    }
}
