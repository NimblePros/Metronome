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
    // Handlers are created by IHttpClientFactory outside the request scope, so the request's
    // counter is found through the current HttpContext. Calls made outside a request (hosted
    // services, startup code) have no counter and are passed through untracked.
    var counter = _accessor.HttpContext?.RequestServices.GetService<HttpCallCounter>();
    if (counter is null)
    {
      return await base.SendAsync(request, cancellationToken);
    }

    var stopwatch = Stopwatch.StartNew();
    var response = await base.SendAsync(request, cancellationToken);
    counter.RecordCall(stopwatch.Elapsed);

    return response;
  }
}
