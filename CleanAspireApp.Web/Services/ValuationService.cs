using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Web.Services;

public class ValuationService
{
    private readonly HttpClient _httpClient;
    public ValuationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //for now I am going to pass the entire endpoint url to this method, but in the future I may want to refactor this to take in parameters and construct the url within the service
    public async Task<PropertyRecord> GetValuationsAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        //var response = await _httpClient.GetAsync(url, cts.Token);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<PropertyRecord>();
        //var data = await response.Content.ReadFromJsonAsync<PropertyRecord>(cancellationToken: cts.Token);
        return data ?? throw new InvalidOperationException("API returned no data.");
    }

    public async Task<List<PropertyRecord>> GetSSValuationsAsync(string schemeName)
    {
        var response = await _httpClient.GetAsync($"/api/valuations/SchemeValuation/{schemeName}");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<List<PropertyRecord>>();
        return data ?? throw new InvalidOperationException("API returned no data.");
    }
}
