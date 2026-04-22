using CleanAspireApp.Domain.Tenders;

namespace CleanAspireApp.Application.UseCases.Tenders.States;

public class TenderState
{

    private readonly Dictionary<string, List<EasyTender>> _cache = new();

    public List<EasyTender>? Get(string key)
        => _cache.TryGetValue(key, out var value) ? value : null;

    public void Set(string key, List<EasyTender> data)
        => _cache[key] = data;
}
