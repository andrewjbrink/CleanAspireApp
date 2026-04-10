namespace CleanAspireApp.Domain.ValuationRoll;

public class PropertyRecord
{
    public string PageNumber { get; set; } = string.Empty;
    public string PropertyReference { get; set; } = string.Empty;
    public string ErfNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RatingCategory { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Extent { get; set; }
    public string MarketValue { get; set; } = string.Empty;
    public string ValuationReason { get; set; } = string.Empty;
    public string ValuationType { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string DisputeExpiryDate { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string HoldingLink { get; set; } = string.Empty;
    public string Erf { get; set; } = string.Empty;
    public string Allotment { get; set; } = string.Empty;
    public List<Sales> SalesRecords { get; set; } = new List<Sales>();
    public List<PropertyRecord> ValuedTogether { get; set; } = new List<PropertyRecord>();
}
