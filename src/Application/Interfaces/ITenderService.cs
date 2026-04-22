using CleanAspireApp.Domain.Tenders;

namespace CleanAspireApp.Application.Interfaces;

public interface ITenderService
{
    Task<List<EasyTender>> GetTendersAsync();
}
