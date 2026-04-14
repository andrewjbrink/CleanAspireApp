using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Web.Services;

public class ValuationStateMany
{
    private readonly Dictionary<string, List<PropertyRecord>> _cache = new();

    public List<PropertyRecord>? Get(string key)
        => _cache.TryGetValue(key, out var value) ? value : null;

    public void Set(string key, List<PropertyRecord> data)
        => _cache[key] = data;
}
