using CleanAspireApp.Application.Common.Behaviours;
using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAspireApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(applicationAssembly);
            config.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));

            // NOTE: Switch to ValidationExceptionBehavior if you want to use exceptions over the result pattern for flow control
            //config.AddOpenBehavior(typeof(ValidationExceptionBehaviour<,>));
            config.AddOpenBehavior(typeof(ValidationErrorOrResultBehavior<,>));

            config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
        services.AddScoped<IPropertyValuation, PropertyValuations>();
        services.AddScoped<ITenderService, TenderService>();

        return services;
    }
}