using CleanAspireApp.Application.UseCases.Tenders.Queries;
using CleanAspireApp.Domain.Tenders;
using CleanAspireApp.WebApi.Extensions;
using MediatR;

namespace CleanAspireApp.WebApi.Endpoints;

public static class TenderEndpoints
{
    public static void MapTenderEndpoints(this WebApplication app)
    {
        var group = app.MapApiGroup("tenders");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
         {
             var results = await sender.Send(new GetTendersQuery(), ct);
             return TypedResults.Ok(results);
         })
          .WithName(nameof(GetTendersQuery))
          .Produces<List<EasyTender>>()
          .WithSummary("Get All Tenders")
          .WithDescription("Retrieves all the Tenders.");
    }
}
