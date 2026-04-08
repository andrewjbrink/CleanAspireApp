using CleanAspireApp.Application.Common.Interfaces;
using System.Security.Claims;

namespace CleanAspireApp.WebApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}