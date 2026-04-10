using Microsoft.JSInterop;

namespace CleanAspireApp.Web.Extensions;

public static class InteropLogger
{
    private static ILogger? s_logger;

    public static void Configure(ILoggerFactory loggerFactory)
    {
        s_logger = loggerFactory.CreateLogger("InteropLogger");
    }

    [JSInvokable("LogFromJS")]
    public static void LogFromJavaScript(string message)
    {
        s_logger?.LogInformation("JS Message: {Message}", message);
        //Console.WriteLine($"JS Message: {message}");
    }
}
