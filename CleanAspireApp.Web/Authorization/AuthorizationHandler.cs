using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CleanAspireApp.Web.Authorization;

public class AuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Implement your authorization logic here
        // 1. Extract access token from the request (e.g., from headers)

        var httpcontext = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("HTTP context is not available");
        var accessToken = await httpcontext.GetTokenAsync("access_token");

        // 2. Attach token to outgoing request

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
