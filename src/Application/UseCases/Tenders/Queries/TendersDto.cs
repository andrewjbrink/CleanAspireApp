namespace CleanAspireApp.Application.UseCases.Tenders.Queries;

public class TendersDto
{
    public int MunicipalityId { get; set; }
    public string TenderNumber { get; set; } = string.Empty;
    public DateOnly OpeningDate { get; set; }
    public DateOnly ClosingDate { get; set; }
    public string TenderUrl { get; set; } = string.Empty;
    public string TenderDocument { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Expired { get; set; }
    public bool OnDatabase { get; set; } = false;
    public bool Captured { get; set; } = false;
    public int Age { get; set; }
    public string MunicipalityName { get; set; } = string.Empty;
}