using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CleanAspireApp.Web.Authorization;

public static class LoginLogoutEndpoints
{
    public static void MapLoginLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/authentication");
        group.MapGet("/login", () =>
        {
            return Results.Challenge(new AuthenticationProperties { RedirectUri = "/" });
        }).AllowAnonymous();

        group.MapPost("/logout", () =>
        {
            return Results.SignOut(authenticationSchemes: [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme]);

        });
    }
}
