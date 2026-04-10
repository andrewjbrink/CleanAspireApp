using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.Interfaces;

public interface IPropertyValuation
{
    Task<List<PropertyRecord>> GetAllValuations(string erf);
    Task<PropertyRecord> GetPropertyValuation(string erf, string allotment);
    Task<PropertyRecord> GetAllFarmValuations(string farm);
    Task<List<PropertyRecord>> GetAllSSValuations(string schemeName);
    Task<List<PropertyRecord>> GetAllSSUnitValuations(string schemeName, string unit);
}
