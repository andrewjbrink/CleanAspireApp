namespace CleanAspireApp.Domain.ValuationRoll;

public class CoCtRollResponse
{
    public List<PropertyRecord> PropertyRecords { get; set; } = new List<PropertyRecord>();
    public List<Sales> SalesRecords { get; set; } = new List<Sales>();
}
