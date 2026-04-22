using Scalar.AspNetCore;

namespace CleanAspireApp.WebApi.Extensions;

public static class ConfigureOpenApi
{
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    public static void AddOpenApiServices(this IServiceCollection services)
    {

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

    }

    public static void AplyOpenApiConfig(this WebApplication app)
    {

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "This is my Scalar API";
            options.DarkMode = true;
            options.Favicon = "path";
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.HideModels = false;
            options.Layout = ScalarLayout.Modern;
            options.ShowSidebar = true;
            options.HideDarkModeToggle = true;
            options.HideModels = true;

            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = ["Bearer"]
            };

        });

    }
}
