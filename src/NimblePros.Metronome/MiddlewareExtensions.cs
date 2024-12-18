using Microsoft.AspNetCore.Builder;

namespace NimblePros.Metronome;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseMetronomeLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CombinedCallLoggingMiddleware>();
    }
}
