using CleanAspireApp.Domain.Tenders;

namespace CleanAspireApp.Web.Services;

public class TenderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenderService> _logger;

    public TenderService(HttpClient httpClient, ILogger<TenderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<EasyTender>> GetTenders()
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/tenders/");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<EasyTender>>();
            _logger.LogInformation("Successfully retrieved tenders from API.");
            return data ?? throw new InvalidOperationException("API returned no data.");
        }

        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tenders from API.");
        }

        catch (InvalidOperationException)
        {
            _logger.LogWarning("API returned no data for tenders. Invalid operation.");
        }

        catch (Polly.Timeout.TimeoutRejectedException)
        {
            _logger.LogWarning("GetTenders timed out (Polly).");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Getting tenders operation canceled.");
        }
        finally
        {
            _logger.LogInformation("GetTenders operation completed.");
        }

        return new List<EasyTender>();
    }
}
