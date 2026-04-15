using CleanAspireApp.Domain.ValuationRoll;

namespace CleanAspireApp.Application.UseCases.Valuations.States;

public class ValuationState
{
    private readonly Dictionary<string, PropertyRecord> _cache = new();

    public PropertyRecord? Get(string key)
        => _cache.TryGetValue(key, out var value) ? value : null;

    public void Set(string key, PropertyRecord data)
        => _cache[key] = data;
}
