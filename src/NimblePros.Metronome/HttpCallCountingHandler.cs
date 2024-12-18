using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace NimblePros.Metronome;

public sealed class HttpCallCountingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCallCountingHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var counter = _accessor.HttpContext.RequestServices
            .GetRequiredService<HttpCallCounter>();
        Console.WriteLine($"counter in http interceptor: {counter.GetHashCode()}");

        Console.WriteLine($"Intercepting request to: {request.RequestUri}");
        var stopwatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        counter.CallCount++;
        counter.TotalTimeMs += stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Request to {request.RequestUri} completed in {stopwatch.ElapsedMilliseconds}ms");

        return response;
    }
}
