using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

    var stopwatch = Stopwatch.StartNew();
    var response = await base.SendAsync(request, cancellationToken);
    stopwatch.Stop();

    counter.CallCount++;
    counter.TotalTimeSpan += stopwatch.Elapsed;

    return response;
  }
}
