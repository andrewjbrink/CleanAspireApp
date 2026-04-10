namespace CleanAspireApp.Web.Extensions;

// TODO: Can we remove this?
// #pragma warning disable IDE0055

public static class DependencyInjection
{
    public static void AddWebApiWeb(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
    }
}
// #pragma warning restore IDE0055
