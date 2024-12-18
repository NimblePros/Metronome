using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NimblePros.Metronome;
public class CombinedCallLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public CombinedCallLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        DbCallCounter dbCounter,
        HttpCallCounter httpCounter,
        ILogger<CombinedCallLoggingMiddleware> logger)
    {
        await _next(context);
        bool verbose = false;
        if (dbCounter.CallCount > 0 || verbose)
        {
            Console.WriteLine($"db counter in middleware: {dbCounter.GetHashCode()}");
            logger.LogInformation($"Database calls: {dbCounter.CallCount}, Total time: {dbCounter.TotalTimeMs} ms");
        }
        if (httpCounter.CallCount > 0 || verbose)
        {
            Console.WriteLine($"http counter in middleware: {httpCounter.GetHashCode()}");
            logger.LogInformation($"HTTP calls: {httpCounter.CallCount}, Total time: {httpCounter.TotalTimeMs} ms");
        }
    }
}