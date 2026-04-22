using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CleanAspireApp.Web.Extensions;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Example: Authenticate a user with a hardcoded username
        var identity = new ClaimsIdentity(
        new[]
        {
new Claim(ClaimTypes.Name, "exampleUser")
        },
        "Custom Authentication");

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    public void AuthenticateUser(string username)
    {
        var identity = new ClaimsIdentity(
        new[]
        {
            new Claim(ClaimTypes.Name, username)
        },
        "Custom Authentication");

        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
